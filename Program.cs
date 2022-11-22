//-----------------------------------------------------------
// <copyright file="Program.cs" company="FH Wiener Neustadt">
//     Copyright (c) FH Wiener Neustadt. All rights reserved.
// </copyright>
// <author>Benjamin Bogner</author>
// <summary>Contains the Program class.</summary>
//-----------------------------------------------------------
namespace CustomArchiver
{
    /// <summary>
    /// Represents the <see cref="Program"/> class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Represents the entry point of the application.
        /// </summary>
        /// <param name="args">Possibly specified command line arguments.</param>
        private static void Main(string[] args)
        {
            ApplicationSettings settings = new ApplicationSettings(args);
            Application application = new Application(settings);
            application.Run();
        }
    }
}
