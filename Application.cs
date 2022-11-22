//---------------------------------------------------------------
// <copyright file="Application.cs" company="FH Wiener Neustadt">
//     Copyright (c) FH Wiener Neustadt. All rights reserved.
// </copyright>
// <author>Benjamin Bogner</author>
// <summary>Contains the Application class.</summary>
//---------------------------------------------------------------
namespace CustomArchiver
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents the <see cref="Application"/> class.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// The <see cref="ApplicationSettings"/> of this <see cref="Application"/>.
        /// </summary>
        private ApplicationSettings settings;

        /// <summary>
        /// Initialises a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="ApplicationSettings"/> of this <see cref="Application"/>.</param>
        public Application(ApplicationSettings settings)
        {
            this.Settings = settings;
        }

        /// <summary>
        /// Gets the <see cref="ApplicationSettings"/> of this <see cref="Application"/>.
        /// </summary>
        /// <value>The <see cref="ApplicationSettings"/> of this <see cref="Application"/>.</value>
        public ApplicationSettings Settings
        {
            get
            {
                return this.settings;
            }

            private set
            {
                this.settings = value ?? throw new ArgumentNullException(nameof(value), "The specified value cannot be null.");
            }
        }

        /// <summary>
        /// Starts the application with the given application settings.
        /// </summary>
        public void Run()
        {
            switch (this.Settings.OperationMode)
            {
                case "create":
                    this.ExecuteCreateCommand();
                    break;
                case "append":
                    this.ExecuteAppendCommand();
                    break;
                case "extract":
                    this.ExecuteExtractCommand();
                    break;
                case "info":
                    this.ExecuteInfoCommand();
                    break;
                case "list":
                    this.ExecuteListCommand();
                    break;
                default:
                    this.PrintErrorMessageManualAndExit("The operation mode must be specified. (-c, -a, -x, -i or -l");
                    break;
            }
        }

        /// <summary>
        /// Creates a new archive according to the specified parameters.
        /// </summary>
        private void ExecuteCreateCommand()
        {
            if (!Directory.Exists(this.Settings.SourcePath))
            {
                this.PrintErrorMessageManualAndExit("The specified source directory must exist.");
            }

            if (File.Exists(this.Settings.DestinationPath))
            {
                this.PrintErrorMessageManualAndExit("The specified target archive file cannot be an existing one.");
            }

            CustomArchiver customArchiver = new CustomArchiver();

            customArchiver.IsRleCompressionActivated = this.Settings.IsRleCompressionActivated;
            customArchiver.NumberOfRetryAttempts = this.Settings.NumberOfRetryAttempts;
            customArchiver.WaitTimeBetweenRetryAttempts = this.Settings.WaitTimeBetweenRetryAttempts * 1000;

            try
            {
                customArchiver.Create(this.Settings.SourcePath, this.Settings.DestinationPath);
                Console.WriteLine("Finished creating.");
            }
            catch (Exception e)
            {
                this.PrintErrorMessageManualAndExit("An error occured while creating the custom archive: " + e.Message);
            }
        }

        /// <summary>
        /// Appends new files and directories to an already existing archive according to the specified paths.
        /// </summary>
        private void ExecuteAppendCommand()
        {
            if (!Directory.Exists(this.Settings.SourcePath))
            {
                this.PrintErrorMessageManualAndExit("The specified source directory must exist.");
            }

            if (!File.Exists(this.Settings.DestinationPath))
            {
                this.PrintErrorMessageManualAndExit("The specified target archive file must exist.");
            }

            CustomArchiver customArchiver = new CustomArchiver();

            customArchiver.IsRleCompressionActivated = this.Settings.IsRleCompressionActivated;
            customArchiver.NumberOfRetryAttempts = this.Settings.NumberOfRetryAttempts;
            customArchiver.WaitTimeBetweenRetryAttempts = this.Settings.WaitTimeBetweenRetryAttempts * 1000;

            try
            {
                customArchiver.Append(this.Settings.DestinationPath, this.Settings.SourcePath);
                Console.WriteLine("Finished appending.");
            }
            catch (Exception e)
            {
                this.PrintErrorMessageManualAndExit("An error occured while appending to the custom archive: " + e.Message);
            }
        }

        /// <summary>
        /// Extracts the files and directories of an archive according to the specified paths.
        /// </summary>
        private void ExecuteExtractCommand()
        {
            if (!File.Exists(this.Settings.SourcePath))
            {
                this.PrintErrorMessageManualAndExit("The specified archive file must exist to extract its content.\n" +
                                                    "The source path of the command is invalid.");
            }
            
            CustomArchiver customArchiver = new CustomArchiver();

            customArchiver.NumberOfRetryAttempts = this.Settings.NumberOfRetryAttempts;
            customArchiver.WaitTimeBetweenRetryAttempts = this.Settings.WaitTimeBetweenRetryAttempts * 1000;

            try
            {
                customArchiver.Extract(this.Settings.SourcePath, this.Settings.DestinationPath);
                Console.WriteLine("Finished extracting.");
            }
            catch (Exception e)
            {
                this.PrintErrorMessageManualAndExit("An error occured while extracting from the custom archive: " + e.Message);
            }
        }

        /// <summary>
        /// Prints the meta information of a specified archive.
        /// </summary>
        private void ExecuteInfoCommand()
        {
            if (!File.Exists(this.Settings.SourcePath))
            {
                this.PrintErrorMessageManualAndExit("The specified archive file must exist to show its meta information.\n" +
                                                    "The source path of the command is invalid.");
            }

            CustomArchiver customArchiver = new CustomArchiver
            {
                NumberOfRetryAttempts = this.Settings.NumberOfRetryAttempts,
                WaitTimeBetweenRetryAttempts = this.Settings.WaitTimeBetweenRetryAttempts * 1000
            };

            try
            {
                CustomArchive customArchive = customArchiver.Retrieve(this.Settings.SourcePath);
                this.PrintCustomArchiveInfo(customArchive);
                Console.WriteLine("Finished printing info.");
            }
            catch (Exception e)
            {
                this.PrintErrorMessageManualAndExit("An error occured while retrieving the meta " +
                                                    "information from the custom archive: " + e.Message);
            }
        }

        /// <summary>
        /// Lists all currently archived files of an archive.
        /// </summary>
        private void ExecuteListCommand()
        {
            if (!File.Exists(this.Settings.SourcePath))
            {
                this.PrintErrorMessageManualAndExit("The specified archive file must exist to list its content.\n" +
                                                    "The source path of the command is invalid.");
            }

            CustomArchiver customArchiver = new CustomArchiver()
            {
                NumberOfRetryAttempts = this.Settings.NumberOfRetryAttempts,
                WaitTimeBetweenRetryAttempts = this.Settings.WaitTimeBetweenRetryAttempts * 1000
            };

            try
            {
                CustomArchive customArchive = customArchiver.Retrieve(this.Settings.SourcePath);
                customArchive.Files.ForEach(x => Console.WriteLine(x.Name));
                Console.WriteLine("Finished printing list.");
            }
            catch (Exception e)
            {
                this.PrintErrorMessageManualAndExit("An error occured while retrieving the meta " +
                                                    "information from the custom archive: " + e.Message);
            }
        }

        /// <summary>
        /// Prints the meta information of the specified <see cref="CustomArchive"/>.
        /// </summary>
        /// <param name="customArchive">The specified <see cref="CustomArchive"/> whose meta information gets printed.</param>
        private void PrintCustomArchiveInfo(CustomArchive customArchive)
        {
            Console.WriteLine("UTC Creation date: " + customArchive.CreationDate);
            Console.WriteLine("Run-length-encoding enabled: " + customArchive.IsRunLengthEncodingEnabled);
            Console.WriteLine("Number of files archived: " + customArchive.FileCount);
            Console.WriteLine("Size of all files archived: " + customArchive.SizeOfAllFiles);
            Console.WriteLine("\n");

            foreach (var file in customArchive.Files)
            {
                if (customArchive.IsRunLengthEncodingEnabled)
                {
                    Console.WriteLine($"File: {file.Name}, uncompressed size: {file.UncompressedSize} bytes, compressed size: {file.CompressedSize} bytes");
                }
                else
                {
                    Console.WriteLine($"File: {file.Name}, uncompressed size: {file.UncompressedSize} bytes");
                }
            }
        }

        /// <summary>
        /// Prints the error message and the application manual.
        /// Terminates the application with the exit code 1 afterwards.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        private void PrintErrorMessageManualAndExit(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            this.Settings.PrintApplicationManual();
            Environment.Exit(1);
        }
    }
}
