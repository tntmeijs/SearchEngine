using System;

namespace Databases
{
    /// <summary>
    /// Base class for all database objects
    /// </summary>
    public abstract class Database
    {
        /// <summary>
        /// All available database types
        /// </summary>
        public enum DatabaseType
        {
            PostgreSQL
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
        /// Attempt to connect to the database using the provided connection URL
        /// </summary>
        /// <param name="connectionString">URL to connect with the database</param>
        public void Connect(string connectionString)
        {}
    }
}
