using System;
using System.Configuration;
using Databases;

namespace WebScraper
{
    /// <summary>
    /// Main application
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// App.config settings as a structure
        /// </summary>
        private struct ProgramConfig
        {
            public string ServerURL;
            public string ServerPort;
            public string DatabaseName;
            public string DatabaseUser;
            public string DatabaseUserPassword;
        }

        /// <summary>
        /// Database used to store the results of the program in
        /// </summary>
        private Database ProgramDatabase;

        /// <summary>
        /// Easily accessible App.config settings
        /// </summary>
        private ProgramConfig Configuration;

        /// <summary>
        /// Retrieve the application settings from App.config
        /// </summary>
        private void TryParseAppConfig()
        {
            var appSettings = ConfigurationManager.AppSettings;

            try
            {
                // The App.config file is expected to have the following fields
                Configuration.ServerURL = appSettings["ServerURL"];
                Configuration.ServerPort = appSettings["ServerPort"];
                Configuration.DatabaseName = appSettings["DatabaseName"];
                Configuration.DatabaseUser = appSettings["DatabaseUser"];
                Configuration.DatabaseUserPassword = appSettings["DatabaseUserPassword"];
            }
            catch (Exception)
            {
                // A missing field is fatal, which is why exceptions are not handled here
                // Crashing the application is preferable over handling any errors
                throw new MissingFieldException("Attempted to read a non-existent App.config field.");
            }
        }

        /// <summary>
        /// Initialize the database
        /// </summary>
        /// <param name="type">Type of database to create</param>
        private void InitializeDatabase(Database.DatabaseType type)
        {
            // Database connection details
            Database.DatabaseConnectionInfo connectionInfo = new Database.DatabaseConnectionInfo();
            connectionInfo.HostName         = Configuration.ServerURL;
            connectionInfo.HostPort         = Configuration.ServerPort;
            connectionInfo.DatabaseName     = Configuration.DatabaseName;
            connectionInfo.UserName         = Configuration.DatabaseUser;
            connectionInfo.UserPassword     = Configuration.DatabaseUserPassword;

            // Create and initialize the database
            ProgramDatabase = Database.CreateInstance(type);
            ProgramDatabase.Initialize(connectionInfo);
        }

        /// <summary>
        /// Create a new main program
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        public Program(string[] args)
        {
            TryParseAppConfig();
            InitializeDatabase(Database.DatabaseType.PostgreSQL);

            //#DEBUG: parse test
            Scraper scraper = new Scraper();
            scraper.ScrapePage("https://reddit.com");
        }

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        static void Main(string[] args)
        {
            _ = new Program(args);
        }
    }
}
