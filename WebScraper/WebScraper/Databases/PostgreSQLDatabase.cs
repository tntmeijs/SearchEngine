using Crawling;

using Npgsql;
using System;

namespace Databases
{
    /// <summary>
    /// PostgreSQL database implementation
    /// </summary>
    internal class PostgreSQLDatabase : Database
    {
        /// <summary>
        /// Database connection string, used to establish a connection with the databse
        /// </summary>
        private string ConnectionString;

        /// <summary>
        /// Construct a valid PostgreSQL Npgsql connection string
        /// </summary>
        /// <param name="connectionInfo">Information needed to establish a connection with the database</param>
        /// <returns>Valid Npgsql connection string</returns>
        protected override string ConstructConnectionString(DatabaseConnectionInfo connectionInfo)
        {
            return string.Format("Host={0};Port={1};Database={2};Username={3};Password={4}",
                connectionInfo.HostName,
                connectionInfo.HostPort,
                connectionInfo.DatabaseName,
                connectionInfo.UserName,
                connectionInfo.UserPassword);
        }

        /// <summary>
        /// Prepare the database for usage
        /// </summary>
        /// <param name="connectionInfo">Connection information</param>
        public override void Initialize(DatabaseConnectionInfo connectionInfo)
        {
            ConnectionString = ConstructConnectionString(connectionInfo);
        }

        /// <summary>
        /// Create a table with the specified name if it does not exist yet
        /// </summary>
        /// <param name="table">Name of the database table to create</param>
        public override void TryCreate(string table)
        {
            try
            {
                string sqlCommand = string.Format(
                    "CREATE TABLE IF NOT EXISTS {0} (" +
                    "   url TEXT PRIMARY KEY NOT NULL," +
                    "   title TEXT NOT NULL," +
                    "   description TEXT NOT NULL," +
                    "   rank REAL NOT NULL," +
                    "   timestamp TIMESTAMP NOT NULL" +
                    ");", table);

                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to create table:\t" + e.Message);
            }
        }

        /// <summary>
        /// Insert a new page into the database or update an existing entry
        /// </summary>
        /// <param name="pageInfo">Information of the page to add</param>
        /// <param name="tableName">Name of the table to insert the page information into</param>
        /// <returns>True when the operation completed successfully, false otherwise</returns>
        public override bool AddPage(PageInfo pageInfo, string tableName)
        {
            bool success = true;

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    string sqlCommand = string.Format(
                        "INSERT INTO {0} (url, title, description, rank, timestamp)" +
                        "VALUES (@pageUrl, @pageTitle, @pageDescription, @pageRank, TO_TIMESTAMP(@pageTimestamp));", tableName);

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sqlCommand, connection))
                    {
                        cmd.Parameters.AddWithValue("pageUrl", pageInfo.Uri.ToString());
                        cmd.Parameters.AddWithValue("pageTitle", pageInfo.Title);
                        cmd.Parameters.AddWithValue("pageDescription", pageInfo.Description);
                        cmd.Parameters.AddWithValue("pageRank", 0.0f);
                        cmd.Parameters.AddWithValue("pageTimestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while adding / updating page information:\t" + e.Message);
                success = false;
            }

            return success;
        }
    }
}
