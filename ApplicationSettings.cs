//-----------------------------------------------------------------------
// <copyright file="ApplicationSettings.cs" company="FH Wiener Neustadt">
//     Copyright (c) FH Wiener Neustadt. All rights reserved.
// </copyright>
// <author>Benjamin Bogner</author>
// <summary>Contains the ApplicationSettings class.</summary>
//-----------------------------------------------------------------------
namespace CustomArchiver
{
    using System;
    using System.Linq;

    /// <summary>
    /// Represents the <see cref="ApplicationSettings"/> class.
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// The destination path of the application settings.
        /// </summary>
        private string destinationPath;

        /// <summary>
        /// The source path of the application settings.
        /// </summary>
        private string sourcePath;

        /// <summary>
        /// The number of retry attempts of the application settings.
        /// </summary>
        private int numberOfRetryAttempts;

        /// <summary>
        /// The wait time between retry attempts of the application settings in seconds.
        /// </summary>
        private int waitTimeBetweenRetryAttempts;

        /// <summary>
        /// The operation mode of the application settings.
        /// </summary>
        private string operationMode;

        /// <summary>
        /// Initialises a new instance of the <see cref="ApplicationSettings"/> class.
        /// </summary>
        /// <param name="args">The command line arguments for the application settings.</param>
        public ApplicationSettings(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("This application requires parameters to be specified in order to execute.\n" +
                                  "Refer to the manual below, for further information.");
                this.PrintApplicationManual();
                Environment.Exit(1);
            }

            // Set necessary default values.
            this.NumberOfRetryAttempts = 1;
            this.WaitTimeBetweenRetryAttempts = 1;

            try
            {
                this.ProcessCommandLineArguments(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while processing the specified command line arguments, " +
                                  "refer to the administrator with the following error message for further information: " +
                                  e.Message + "\n");
                this.PrintApplicationManual();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Gets the current operation mode of the application settings.
        /// </summary>
        /// <value>The current operation mode of the application setting,.</value>
        public string OperationMode
        {
            get
            {
                return this.operationMode;
            }

            private set
            {
                if (this.operationMode != null)
                {
                    throw new ApplicationException(
                        "This application cannot be executed with multiple operation modes at the same time.");
                }

                this.operationMode = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the run-length-encoding is activated or not.
        /// </summary>
        /// <value>The value indicating whether the run-length-encoding is activated or not.</value>
        public bool IsRleCompressionActivated
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of retry attempts of the application settings.
        /// </summary>
        /// <value>The number of retry attempts of the application settings.</value>
        public int NumberOfRetryAttempts
        {
            get
            {
                return this.numberOfRetryAttempts;
            }

            private set
            {
                if (value < 1 || value > 10)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        "The specified retry attempts must be within 1 and 10. (including borders)");
                }

                this.numberOfRetryAttempts = value;
            }
        }

        /// <summary>
        /// Gets the wait time between retry attempts of the application settings in seconds.
        /// </summary>
        /// <value>The wait time between retry attempts of the application settings in seconds.</value>
        public int WaitTimeBetweenRetryAttempts
        {
            get
            {
                return this.waitTimeBetweenRetryAttempts;
            }

            private set
            {
                if (value < 1 || value > 10)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        "The specified wait time between retry attempts must be within 1 and 10. (including borders)");
                }

                this.waitTimeBetweenRetryAttempts = value;
            }
        }

        /// <summary>
        /// Gets the source path of the application settings.
        /// </summary>
        /// <value>The source path of the applications settings.</value>
        public string SourcePath
        {
            get
            {
                return this.sourcePath;
            }

            private set
            {
                if (value == string.Empty)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified path cannot be empty.");
                }

                this.sourcePath = value;
            }
        }

        /// <summary>
        /// Gets the destination path of the application settings.
        /// </summary>
        /// <value>The destination path of the application settings.</value>
        public string DestinationPath
        {
            get
            {
                return this.destinationPath;
            }

            private set
            {
                if (value == string.Empty)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified path cannot be empty.");
                }

                this.destinationPath = value;
            }
        }

        /// <summary>
        /// Prints the application manual in the <see cref="Console"/> window.
        /// </summary>
        public void PrintApplicationManual()
        {
            Console.WriteLine("\n\nCustom_Archiver-Manual");
            Console.WriteLine("____________________________________________________________");
            Console.WriteLine("This application can be used to create a custom archive,\n" +
                              "append to an existing custom archive or extract from an\n" +
                              "existing custom archive. Additionally meta information about\n" +
                              "an existing custom archive can be retrieved, or the content\n" +
                              "can be displayed in a list. The custom archive supports\n" +
                              "archiving using run-length-encoding.\n");

            Console.WriteLine("To use this Custom_Archiver, execute it with proper parameters:\n" +
                              "(Attention: Only one main operation mode per execution allowed!)\n");

            Console.WriteLine("-c or --create\n" +
                              "______________\n" +
                              "Main operation mode for creating a new custom archive. (Requires -s and -d)\n");

            Console.WriteLine("-a or --append\n" +
                              "______________\n" +
                              "Main operation mode for appending to an existing custom archive. (Requires -s and -d)\n");

            Console.WriteLine("-x or --extract\n" +
                              "_______________\n" +
                              "Main operation mode for extracting from an existing custom archive. (Requires -s and -d)\n");

            Console.WriteLine("-rle or --rleCompress\n" +
                              "_____________________\n" +
                              "Adding this parameter enables run-length-encoding for file contents.\n");

            Console.WriteLine("-i or --info\n" +
                              "____________\n" +
                              "Main operation mode for showing meta information of an existing custom archive. (Requires -s)\n");

            Console.WriteLine("-l or --list\n" +
                              "____________\n" +
                              "Main operation mode for listing all names of files of an existing custom archive. (Requires -s)\n");

            Console.WriteLine("-r or --retry\n" +
                              "_____________\n" +
                              "Specifies the number of retry attempts, in case of failed file access.\n" +
                              "Default value is 1, legal values range from 1 to 10. (Example: -r 3)\n");

            Console.WriteLine("-w or --wait\n" +
                              "____________\n" +
                              "Specifies the wait time between retry attempts in seconds.\n" +
                              "Default value is 1, legal values range from 1 to 10. (Example: -w 5)\n");

            Console.WriteLine("-s or --source\n" +
                              "______________\n" +
                              "Specifies the source path for operations. (Directory for -c and -a / File for -x)\n");

            Console.WriteLine("-d or --destination\n" +
                              "___________________\n" +
                              "Specifies the destination path for operations. (File for -c and -a / Directory for -x)");
        }

        /// <summary>
        /// Processes the command line arguments and sets the application settings properties according to it.
        /// </summary>
        /// <param name="args">The specified command line arguments to be processed.</param>
        private void ProcessCommandLineArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-c":
                    case "--create":
                        this.OperationMode = "create";
                        break;
                    case "-a":
                    case "--append":
                        this.OperationMode = "append";
                        break;
                    case "-x":
                    case "--extract":
                        this.OperationMode = "extract";
                        break;
                    case "-rle":
                    case "--rleCompress":
                        this.IsRleCompressionActivated = true;
                        break;
                    case "-i":
                    case "--info":
                        this.OperationMode = "info";
                        break;
                    case "-l":
                    case "--list":
                        this.OperationMode = "list";
                        break;
                    case "-r":
                    case "--retry":
                        this.ProcessRetryCommand(args, i);
                        break;
                    case "-w":
                    case "--wait":
                        this.ProcessWaitTimeCommand(args, i);
                        break;
                    case "-s":
                    case "--source":
                        this.ProcessSourceCommand(args, i);
                        break;
                    case "-d":
                    case "--destination":
                        this.ProcessDestinationCommand(args, i);
                        break;
                }
            }
        }

        /// <summary>
        /// Processes the retry command of the command line arguments.
        /// </summary>
        /// <param name="args">The specified command line arguments.</param>
        /// <param name="currentIndex">The current index of the command line processing.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Is raised when the number of retry attempts is either not specified or out of range.
        /// </exception>
        private void ProcessRetryCommand(string[] args, int currentIndex)
        {
            // If the next value is out of range.
            if (currentIndex + 1 > args.Length - 1)
            {
                throw new ArgumentException("The retry command requires a parameter.");
            }

            try
            {
                this.NumberOfRetryAttempts = int.Parse(args[currentIndex + 1]);
            }
            catch (Exception)
            {
                throw new ArgumentOutOfRangeException("The number of retry attempts was either not specified " +
                                                      "or outside of the legal range. Refer to the application " +
                                                      "manual for further information.");
            }
        }

        /// <summary>
        /// Processes the wait command of the command line arguments.
        /// </summary>
        /// <param name="args">The specified command line arguments.</param>
        /// <param name="currentIndex">The current index of the command line processing.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Is raised when the wait time is either not specified or out of range.
        /// </exception>
        private void ProcessWaitTimeCommand(string[] args, int currentIndex)
        {
            // If the next value is out of range.
            if (currentIndex + 1 > args.Length - 1)
            {
                throw new ArgumentException("The wait time command requires a parameter.");
            }

            try
            {
                this.WaitTimeBetweenRetryAttempts = int.Parse(args[currentIndex + 1]);
            }
            catch (Exception)
            {
                throw new ArgumentOutOfRangeException("The specified wait time between retry attempts was either" +
                                                      "not specified or outside of the legal range. Refer to the " +
                                                      "application manual for further information.");
            }
        }

        /// <summary>
        /// Processes the source command of the command line arguments.
        /// </summary>
        /// <param name="args">The specified command line arguments.</param>
        /// <param name="currentIndex">The current index of the command line processing.</param>
        private void ProcessSourceCommand(string[] args, int currentIndex)
        {
            // If the next value is out of range.
            if (currentIndex + 1 > args.Length - 1)
            {
                throw new ArgumentException("The source command requires a parameter.");
            }

            this.SourcePath = args[currentIndex + 1];
        }

        /// <summary>
        /// Processes the destination command of the command line arguments.
        /// </summary>
        /// <param name="args">The specified command line arguments.</param>
        /// <param name="currentIndex">The current index of the command line processing.</param>
        private void ProcessDestinationCommand(string[] args, int currentIndex)
        {
            // If the next value is out of range.
            if (currentIndex + 1 > args.Length - 1)
            {
                throw new ArgumentException("The destination command requires a parameter.");
            }

            this.DestinationPath = args[currentIndex + 1];
        }
    }
}
