using System;

using Crawling;

namespace Databases
{
    /// <summary>
    /// Base class for all database objects
    /// </summary>
    internal abstract class Database
    {
        /// <summary>
        /// All available database types
        /// </summary>
        public enum DatabaseType
        {
            PostgreSQL
        }

        /// <summary>
        /// Database connection information structure
        /// </summary>
        public struct DatabaseConnectionInfo
        {
            public string HostName;
            public string HostPort;
            public string DatabaseName;
            public string TableName;
            public string UserName;
            public string UserPassword;
        }

        protected readonly DatabaseConnectionInfo ConnectionInfo;

        /// <summary>
        /// Database connection string, used to establish a connection with the database
        /// </summary>
        protected string ConnectionString;

        /// <summary>
        /// Create a new database object
        /// </summary>
        /// <param name="type">Database to create</param>
        /// <param name="connectionInfo">Information needed to connect to the database</param>
        /// <returns>New database</returns>
        public static Database CreateInstance(DatabaseType type, DatabaseConnectionInfo connectionInfo)
        {
            switch (type)
            {
                case DatabaseType.PostgreSQL:
                    return new PostgreSQLDatabase(connectionInfo);
                default:
                    throw new ArgumentException("Invalid database type.");
            }
        }

        /// <summary>
        /// Helper function to turn a "DatabaseConnectionInfo" structure into a
        /// database connection string
        /// </summary>
        /// <returns>Connection string</returns>
        protected abstract string ConstructConnectionString();

        /// <summary>
        /// Attempt to create a new database or do nothing if one with the same
        /// name already exists
        /// </summary>
        protected abstract void Create();

        /// <summary>
        /// Create a new database and initialize it
        /// </summary>
        /// <param name="connectionInfo">Information needed to connect to the database</param>
        public Database(DatabaseConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
            ConnectionString = ConstructConnectionString();
            Create();
        }

        /// <summary>
        /// Add a new page to the database or update an existing entry with the
        /// latest information
        /// </summary>
        /// <param name="pageInfo">Page information retrieved from the web crawler</param>
        /// <param name="tableName">Name of the table to insert the page information into</param>
        /// <returns>True when the operation completed successfully, false otherwise</returns>
        public abstract bool AddOrUpdateCrawledPage(PageInfo pageInfo, string tableName);

        /// <summary>
        /// Add a new page to the database table with URLs that are pending a crawl
        /// </summary>
        /// <param name="urls">URLs to store</param>
        /// <param name="tableName">Name of the table that contains pending URLs</param>
        /// <returns>True when the operation completed successfully, false otherwise</returns>
        public abstract bool TryAddPendingUrls(string[] urls, string tableName);

        /// <summary>
        /// Retrieve URLs that have not been crawled yet
        /// </summary>
        /// <param name="count">Maximum number of URLs to retrieve</param>
        /// <param name="tableName">Name of the table to retrieve the URLs from</param>
        /// <returns>Array of URLs</returns>
        public abstract string[] GetUncrawledUrls(int count, string tableName);
    }
}
