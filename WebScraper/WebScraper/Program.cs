using System;
using System.Configuration;

using Crawling;
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
            public string TableName;
            public string PendingCrawlTableName;
            public string DatabaseUser;
            public string DatabaseUserPassword;

            public int MinCrawlDelay;
            public int MaxCrawlDelay;
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
                Configuration.ServerURL             = appSettings["ServerURL"];
                Configuration.ServerPort            = appSettings["ServerPort"];
                Configuration.DatabaseName          = appSettings["DatabaseName"];
                Configuration.TableName             = appSettings["TableName"];
                Configuration.PendingCrawlTableName = appSettings["PendingCrawlTableName"];
                Configuration.DatabaseUser          = appSettings["DatabaseUser"];
                Configuration.DatabaseUserPassword  = appSettings["DatabaseUserPassword"];

                Configuration.MinCrawlDelay         = int.Parse(appSettings["MinCrawlDelay"]);
                Configuration.MaxCrawlDelay         = int.Parse(appSettings["MaxCrawlDelay"]);
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
            Database.DatabaseConnectionInfo connectionInfo = new Database.DatabaseConnectionInfo
            {
                HostName = Configuration.ServerURL,
                HostPort = Configuration.ServerPort,
                DatabaseName = Configuration.DatabaseName,
                UserName = Configuration.DatabaseUser,
                UserPassword = Configuration.DatabaseUserPassword
            };

            // Create and initialize the database
            ProgramDatabase = Database.CreateInstance(type);
            ProgramDatabase.Initialize(connectionInfo);
            ProgramDatabase.TryCreate(Configuration.TableName, Configuration.PendingCrawlTableName);
        }

        /// <summary>
        /// Create a new main program
        /// </summary>
        public Program()
        {
            TryParseAppConfig();
            InitializeDatabase(Database.DatabaseType.PostgreSQL);

            // Start crawling
            Crawler crawler = new Crawler();
            crawler.Start(Configuration.MinCrawlDelay, Configuration.MaxCrawlDelay, ProgramDatabase, Configuration.TableName, Configuration.PendingCrawlTableName);
        }

        /// <summary>
        /// Application entry point
        /// </summary>
        static void Main()
        {
            _ = new Program();
        }
    }
}
