﻿using System;

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
            public string UserName;
            public string UserPassword;
        }

        /// <summary>
        /// Create a new database object
        /// </summary>
        /// <param name="type">Database to create</param>
        /// <returns>New database</returns>
        public static Database CreateInstance(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.PostgreSQL:
                    return new PostgreSQLDatabase();
                default:
                    throw new ArgumentException("Invalid database type.");
            }
        }

        /// <summary>
        /// Helper function to turn a "DatabaseConnectionInfo" structure into a
        /// database connection string
        /// </summary>
        /// <param name="connectionInfo">Information needed to establish a connection with the database</param>
        /// <returns>Connection string</returns>
        protected abstract string ConstructConnectionString(DatabaseConnectionInfo connectionInfo);

        /// <summary>
        /// Initialize the database object to prepare for usage
        /// </summary>
        /// <param name="connectionInfo">Information needed to establish a connection with the database</param>
        public abstract void Initialize(DatabaseConnectionInfo connectionInfo);

        /// <summary>
        /// Attempt to create a new database or do nothing if one with the same
        /// name already exists
        /// </summary>
        /// <param name="tableName">Name of the database table to create</param>
        public abstract void TryCreate(string tableName);

        /// <summary>
        /// Add a new page to the database or update an existing entry with the
        /// latest information
        /// </summary>
        /// <param name="pageInfo">Page information retrieved from the web crawler</param>
        /// <param name="tableName">Name of the table to insert the page information into</param>
        /// <returns>True when the operation completed successfully, false otherwise</returns>
        public abstract bool AddPage(PageInfo pageInfo, string tableName);
    }
}
