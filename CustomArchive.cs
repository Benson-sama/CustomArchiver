//-----------------------------------------------------------------
// <copyright file="CustomArchive.cs" company="FH Wiener Neustadt">
//     Copyright (c) FH Wiener Neustadt. All rights reserved.
// </copyright>
// <author>Benjamin Bogner</author>
// <summary>Contains the CustomArchive class.</summary>
//-----------------------------------------------------------------
namespace CustomArchiver
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents the <see cref="CustomArchive"/> class.
    /// </summary>
    [Serializable]
    public class CustomArchive
    {
        /// <summary>
        /// The number of files in this <see cref="CustomArchive"/>.
        /// </summary>
        private int fileCount;

        /// <summary>
        /// The size of all files in this <see cref="CustomArchive"/>.
        /// </summary>
        private long sizeOfAllFiles;

        /// <summary>
        /// The list of all files stored in this <see cref="CustomArchive"/>.
        /// </summary>
        [XmlElement(IsNullable = false)]
        private List<EnhancedFileInfo> files;

        /// <summary>
        /// The list of all folders stored in the meta information of this <see cref="CustomArchive"/>.
        /// </summary>
        [XmlElement(IsNullable = false)]
        private List<string> folders;

        /// <summary>
        /// Initialises a new instance of the <see cref="CustomArchive"/> class.
        /// </summary>
        public CustomArchive()
        {
            this.Files = new List<EnhancedFileInfo>();
            this.Folders = new List<string>();
        }

        /// <summary>
        /// Gets or sets the list containing all files inside this <see cref="CustomArchive"/>.
        /// </summary>
        /// <value>The list containing all files inside this <see cref="CustomArchive"/>.</value>
        [XmlElement(IsNullable = false)]
        public List<EnhancedFileInfo> Files
        {
            get
            {
                return this.files;
            }

            set
            {
                this.files = value ?? throw new ArgumentNullException(nameof(value), "The specified value cannot be null.");
            }
        }

        /// <summary>
        /// Gets or sets the list containing all folders inside this <see cref="CustomArchive"/>.
        /// </summary>
        /// <value>The list containing all folders inside this <see cref="CustomArchive"/>.</value>
        [XmlElement(IsNullable = false)]
        public List<string> Folders
        {
            get
            {
                return this.folders;
            }

            set
            {
                this.folders = value ?? throw new ArgumentNullException(nameof(value), "The specified value cannot be null.");
            }
        }

        /// <summary>
        /// Gets or sets the UTC creation date of the custom archive.
        /// </summary>
        /// <value>The UTC creation date of the custom archive.</value>
        public DateTime CreationDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the run-length-encoding is enabled.
        /// </summary>
        /// <value>The value indicating whether the run-length-encoding is enabled.</value>
        public bool IsRunLengthEncodingEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of files archived in the custom archive.
        /// </summary>
        /// <value>The number of files archived in the custom archive.</value>
        public int FileCount
        {
            get
            {
                return this.fileCount;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified value cannot be less than 1.");
                }

                this.fileCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of all files archived.
        /// </summary>
        /// <value>The size of all files archived.</value>
        public long SizeOfAllFiles
        {
            get
            {
                return this.sizeOfAllFiles;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified value cannot be less than 1.");
                }

                this.sizeOfAllFiles = value;
            }
        }

        /// <summary>
        /// Adds information about the files and folders of the given source directory to this <see cref="CustomArchive"/>.
        /// </summary>
        /// <param name="sourceDirectory">The given source directory.</param>
        public void AddFolders(string sourceDirectory)
        {
            List<string> directories = Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories).ToList();

            foreach (var directory in directories)
            {
                var relativeDirectory = directory.GetRelativeDirectory(sourceDirectory);

                if (!this.Folders.Contains(relativeDirectory))
                {
                    this.Folders.Add(relativeDirectory);
                }
            }
        }

        /// <summary>
        /// Adds files from the given source directory. Ignores files with similar relative paths.
        /// </summary>
        /// <param name="sourceDirectory">The given source directory.</param>
        /// <returns>The list containing the files that have been added.</returns>
        public List<EnhancedFileInfo> AddFiles(string sourceDirectory)
        {
            List<string> filePaths = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories).ToList();
            List<EnhancedFileInfo> newFiles = new List<EnhancedFileInfo>();

            foreach (var filePath in filePaths)
            {
                EnhancedFileInfo file = new EnhancedFileInfo(filePath, sourceDirectory);

                if (!this.Files.Exists(x => x.RelativePath == file.RelativePath))
                {
                    this.Files.Add(file);
                    newFiles.Add(file);
                }
            }

            return newFiles;
        }
    }
}
