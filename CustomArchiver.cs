//------------------------------------------------------------------
// <copyright file="CustomArchiver.cs" company="FH Wiener Neustadt">
//     Copyright (c) FH Wiener Neustadt. All rights reserved.
// </copyright>
// <author>Benjamin Bogner</author>
// <summary>Contains the CustomArchiver class.</summary>
//------------------------------------------------------------------
namespace CustomArchiver
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents the <see cref="CustomArchiver"/> class.
    /// </summary>
    public class CustomArchiver
    {
        /// <summary>
        /// The number of retry attempts of the <see cref="CustomArchiver"/>.
        /// </summary>
        private int numberOfRetryAttempts;

        /// <summary>
        /// The wait time between retry attempts of the <see cref="CustomArchiver"/> in milliseconds.
        /// </summary>
        private int waitTimeBetweenRetryAttempts;

        /// <summary>
        /// Gets or sets the number of retry attempts of the <see cref="CustomArchiver"/>.
        /// </summary>
        /// <value>The number of retry attempts of the <see cref="CustomArchiver"/>.</value>
        public int NumberOfRetryAttempts
        {
            get
            {
                return this.numberOfRetryAttempts;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified number of retry attempts cannot be less than zero.");
                }

                this.numberOfRetryAttempts = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait time between retry attempts of the <see cref="CustomArchiver"/> in milliseconds.
        /// </summary>
        /// <value>The wait time between retry attempts of the <see cref="CustomArchiver"/> in milliseconds.</value>
        public int WaitTimeBetweenRetryAttempts
        {
            get
            {
                return this.waitTimeBetweenRetryAttempts;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified wait time between retry attempts must be greater than zero.");
                }

                this.waitTimeBetweenRetryAttempts = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the run-length-encoding is activated or not.
        /// </summary>
        /// <value>The value indicating whether the run-length-encoding is activated or not.</value>
        public bool IsRleCompressionActivated
        {
            get;
            set;
        }

        /// <summary>
        /// Creates the file of the <see cref="CustomArchive"/> with the given source and destination paths.
        /// </summary>
        /// <param name="sourceDirectory">The source directory path containing the content to be archived.</param>
        /// <param name="archivePath">The file path where the <see cref="CustomArchive"/> file will be saved.</param>
        /// <returns>The created <see cref="CustomArchive"/>.</returns>
        public CustomArchive Create(string sourceDirectory, string archivePath)
        {
            CustomArchive customArchive = new CustomArchive();
            customArchive.IsRunLengthEncodingEnabled = this.IsRleCompressionActivated;
            customArchive.CreationDate = DateTime.UtcNow;
            customArchive.AddFolders(sourceDirectory);
            customArchive.AddFiles(sourceDirectory);

            var archiveFileStream = this.GetFileStream(archivePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
            archiveFileStream.Seek(8, SeekOrigin.Begin);
            this.ArchiveFileContents(customArchive.Files, customArchive, archiveFileStream);

            return customArchive;
        }

        /// <summary>
        /// Appends the content of the given directory path to the existing <see cref="CustomArchive"/>.
        /// </summary>
        /// <param name="archivePath">The path of the existing <see cref="CustomArchive"/>.</param>
        /// <param name="sourcePath">The source directory containing the content to append to the <see cref="CustomArchive"/>.</param>
        /// <returns>The updated <see cref="CustomArchive"/>.</returns>
        public CustomArchive Append(string archivePath, string sourcePath)
        {
            CustomArchive archive = this.Retrieve(archivePath);

            if (archive.IsRunLengthEncodingEnabled != this.IsRleCompressionActivated)
            {
                throw new InvalidOperationException(
                    "Cannot use a run-length-encoding setting different to the one set in the archive.");
            }

            archive.AddFolders(sourcePath);
            var newFiles = archive.AddFiles(sourcePath);

            using (FileStream archiveFileStream = this.GetFileStream(archivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                long metaInformationPosition = this.GetMetaInformationPosition(archiveFileStream);
                archiveFileStream.Seek(metaInformationPosition, SeekOrigin.Begin);
                this.ArchiveFileContents(newFiles, archive, archiveFileStream);
            }

            return archive;
        }

        /// <summary>
        /// Extracts the file content of an already existing archive, recreating stored files while not overwriting existing files.
        /// </summary>
        /// <param name="archivePath">The path of the existing <see cref="CustomArchive"/>.</param>
        /// <param name="destinationPath">The destination directory path of the extraction process.</param>
        /// <returns>A value indicating whether the <see cref="CustomArchive"/> could be extracted.</returns>
        public bool Extract(string archivePath, string destinationPath)
        {
            CustomArchive archive = this.Retrieve(archivePath);
            FileStream archiveFileStream = this.GetFileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.None);
            
            archiveFileStream.Seek(8, SeekOrigin.Begin);

            foreach (var file in archive.Files)
            {
                string filePath = destinationPath + file.RelativePath;
             
                Console.WriteLine($"Extracting {file.Name} ...");

                if (archive.IsRunLengthEncodingEnabled)
                {
                    this.ExtractFileContentRLECompressed(file, archiveFileStream, filePath);
                }
                else
                {
                    this.ExtractFileContentUncompressed(file, archiveFileStream, filePath);
                }
            }

            archiveFileStream.Flush();
            archiveFileStream.Close();

            return true;
        }

        /// <summary>
        /// Retrieves the meta information of an existing <see cref="CustomArchive"/>.
        /// This method does not manipulate the <see cref="CustomArchive"/> file.
        /// </summary>
        /// <param name="archivePath">The path of the existing <see cref="CustomArchive"/>.</param>
        /// <returns>The retrieved <see cref="CustomArchive"/> instance.</returns>
        /// <exception cref="Exception">
        /// Is thrown when the specified <see cref="CustomArchive"/> file could not be loaded.
        /// </exception>
        public CustomArchive Retrieve(string archivePath)
        {
            FileStream fs = this.GetFileStream(archivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            XmlSerializer formatter = new XmlSerializer(typeof(CustomArchive));

            try
            {
                fs.Seek(0, SeekOrigin.Begin);
                long metaInformationPosition = this.GetMetaInformationPosition(fs);
                fs.Seek(metaInformationPosition, SeekOrigin.Begin);
                return (CustomArchive)formatter.Deserialize(fs);
            }
            catch (Exception)
            {
                throw new Exception("Failed to load archive, perhaps the given file is corrupted.");
            }
            finally
            {
                fs?.Flush();
                fs?.Close();
            }
        }

        /// <summary>
        /// Extracts the content of a file from the specified <see cref="FileStream"/> using run-length-encoding,
        /// creates a new <see cref="File"/> and writes the content to it.
        /// </summary>
        /// <param name="file">The file to be extracted.</param>
        /// <param name="archiveFileStream">The source <see cref="FileStream"/>.</param>
        /// <param name="filePath">The filePath for the new file.</param>
        private void ExtractFileContentRLECompressed(EnhancedFileInfo file, FileStream archiveFileStream, string filePath)
        {
            if (File.Exists(filePath))
            {
                return;
            }

            this.CreateDirectoryIfMissing(filePath);

            FileStream destination = this.GetFileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            byte[] buffer = new byte[8192];
            long bytes = file.CompressedSize;

            do
            {
                int nextBytes = bytes > buffer.Length ? buffer.Length : Convert.ToInt32(bytes);
                archiveFileStream.Read(buffer, 0, nextBytes);
                byte[] decodedBuffer = buffer.DecodeRLE();
                destination.Write(decodedBuffer, 0, decodedBuffer.Length);
                bytes -= nextBytes;
            }
            while (bytes > 0);

            destination.Flush();
            destination.Close();
        }

        /// <summary>
        /// Extracts the content of a file from the specified <see cref="FileStream"/>, creates a new <see cref="File"/>
        /// and writes the content to it.
        /// </summary>
        /// <param name="file">The file to be extracted.</param>
        /// <param name="archiveFileStream">The source <see cref="FileStream"/>.</param>
        /// <param name="filePath">The filePath for the new file.</param>
        private void ExtractFileContentUncompressed(EnhancedFileInfo file, FileStream archiveFileStream, string filePath)
        {
            if (File.Exists(filePath))
            {
                return;
            }

            this.CreateDirectoryIfMissing(filePath);

            FileStream destination = this.GetFileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            byte[] buffer = new byte[8192];
            long bytes = file.UncompressedSize;

            do
            {
                int nextBytes = bytes > buffer.Length ? buffer.Length : Convert.ToInt32(bytes);
                archiveFileStream.Read(buffer, 0, nextBytes);
                destination.Write(buffer, 0, nextBytes);
                bytes -= nextBytes;
            }
            while (bytes > 0);

            destination.Flush();
            destination.Close();
        }

        /// <summary>
        /// Writes the content of all files to the specified <see cref="FileStream"/> and adds the meta information to the
        /// specified <see cref="CustomArchive"/>.
        /// </summary>
        /// <param name="files">The files whose contents get archived.</param>
        /// <param name="archive">The <see cref="CustomArchive"/> receiving the meta information.</param>
        /// <param name="archiveFileStream">The target <see cref="FileStream"/> for writing the file contents.</param>
        private void ArchiveFileContents(List<EnhancedFileInfo> files, CustomArchive archive, FileStream archiveFileStream)
        {
            foreach (var file in files)
            {
                if (this.IsRleCompressionActivated)
                {
                    Console.WriteLine($"Archiving run-length-encoded {file.AbsolutePath} ...");
                    this.WriteFileContentRLECompressed(file, archiveFileStream);
                    archive.SizeOfAllFiles += file.CompressedSize;
                }
                else
                {
                    Console.WriteLine($"Archiving uncompressed {file.AbsolutePath} ...");
                    this.WriteFileContentUncompressed(file, archiveFileStream);
                    archive.SizeOfAllFiles += file.UncompressedSize;
                }

                archive.FileCount++;
            }

            this.WriteMetaInformation(archive, archiveFileStream);
        }

        /// <summary>
        /// Writes the file content to the specified <see cref="FileStream"/> uncompressed.
        /// </summary>
        /// <param name="file">The file whose content gets written to the <see cref="FileStream"/>.</param>
        /// <param name="archiveFileStream">The target <see cref="FileStream"/>.</param>
        private void WriteFileContentUncompressed(EnhancedFileInfo file, FileStream archiveFileStream)
        {
            FileStream source = this.GetFileStream(file.AbsolutePath, FileMode.Open, FileAccess.Read, FileShare.None);
            source.Seek(0, SeekOrigin.Begin);

            long totalBytes = 0;
            int currentBytes;

            do
            {
                byte[] buffer = new byte[8192];
                currentBytes = source.Read(buffer, 0, buffer.Length);
                archiveFileStream.Write(buffer, 0, currentBytes);
                totalBytes += currentBytes;
            }
            while (currentBytes > 0);

            source.Flush();
            source.Close();
        }

        /// <summary>
        /// Writes the file content to the specified <see cref="FileStream"/> using run-length-encoding.
        /// </summary>
        /// <param name="file">The file whose content gets written to the <see cref="FileStream"/>.</param>
        /// <param name="archiveFileStream">The target <see cref="FileStream"/>.</param>
        private void WriteFileContentRLECompressed(EnhancedFileInfo file, FileStream archiveFileStream)
        {
            FileStream source = this.GetFileStream(file.AbsolutePath, FileMode.Open, FileAccess.Read, FileShare.None);
            source.Seek(0, SeekOrigin.Begin);

            long totalBytes = 0;
            int currentBytes;

            do
            {
                byte[] buffer = new byte[8192];
                currentBytes = source.Read(buffer, 0, buffer.Length);
                buffer = buffer.Take(currentBytes).ToArray().EncodeRLE();
                currentBytes = buffer.Length;
                archiveFileStream.Write(buffer, 0, currentBytes);
                totalBytes += currentBytes;
            }
            while (currentBytes > 0);

            source.Flush();
            source.Close();

            file.CompressedSize = totalBytes;
        }

        /// <summary>
        /// Writes the meta information of a <see cref="CustomArchive"/> into the given <see cref="FileStream"/>.
        /// </summary>
        /// <param name="archive">The given <see cref="CustomArchive"/>.</param>
        /// <param name="archiveFileStream">The target <see cref="FileStream"/>.</param>
        private void WriteMetaInformation(CustomArchive archive, FileStream archiveFileStream)
        {
            byte[] metaInformationPosition = BitConverter.GetBytes(archiveFileStream.Position);

            XmlSerializer formatter = new XmlSerializer(typeof(CustomArchive));
            formatter.Serialize(archiveFileStream, archive);

            archiveFileStream.Seek(0, SeekOrigin.Begin);
            archiveFileStream.Write(metaInformationPosition, 0, metaInformationPosition.Length);
        }

        /// <summary>
        /// Gets the meta information of a <see cref="CustomArchive"/> from the given <see cref="FileStream"/>.
        /// </summary>
        /// <param name="archiveFileStream">The source <see cref="FileStream"/> for the <see cref="CustomArchive"/>.</param>
        /// <returns>The 64-bit signed integer representing the position of the meta information.</returns>
        private long GetMetaInformationPosition(FileStream archiveFileStream)
        {
            archiveFileStream.Seek(0, SeekOrigin.Begin);
            byte[] dataOfMetaInformationPosition = new byte[8];
            archiveFileStream.Read(dataOfMetaInformationPosition, 0, 8);

            return BitConverter.ToInt64(dataOfMetaInformationPosition, 0);
        }

        /// <summary>
        /// Gets the <see cref="FileStream"/> using the given parameters.
        /// </summary>
        /// <param name="filePath">The file path for the <see cref="FileStream"/>.</param>
        /// <param name="fileMode">The file mode for the <see cref="FileStream"/>.</param>
        /// <param name="fileAccess">The file access rights for the <see cref="FileStream"/>.</param>
        /// <param name="fileShare">The file share rights for the <see cref="FileStream"/>.</param>
        /// <returns>The desired <see cref="FileStream"/>.</returns>
        /// <exception cref="IOException">
        /// Is raised when the <see cref="FileStream"/> could not be retrieved.
        /// </exception>
        private FileStream GetFileStream(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            int retryAttempts = this.NumberOfRetryAttempts;

            while (retryAttempts > 0)
            {
                try
                {
                    return new FileStream(filePath, fileMode, fileAccess, fileShare);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to {fileMode.ToString().ToLower()} file.");

                    if (retryAttempts > 1)
                    {
                        Console.WriteLine($"Waiting {this.WaitTimeBetweenRetryAttempts / 1000} seconds until next attempt.");
                        Thread.Sleep(this.WaitTimeBetweenRetryAttempts);
                    }
                }
                finally
                {
                    retryAttempts--;
                }
            }

            throw new IOException($"Could not {fileMode.ToString().ToLower()} using the path: {filePath}");
        }

        /// <summary>
        /// Checks if the given path is an existing directory. If not the directory gets created.
        /// </summary>
        /// <param name="path">The path of the directory.</param>
        private void CreateDirectoryIfMissing(string path)
        {
            FileInfo fileInfo = new FileInfo(path);

            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
        }
    }
}
