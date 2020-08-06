using Crawling;

using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Databases
{
    /// <summary>
    /// PostgreSQL database implementation
    /// </summary>
    internal class PostgreSQLDatabase : Database
    {
        /// <summary>
        /// Construct a valid PostgreSQL Npgsql connection string
        /// </summary>
        /// <param name="connectionInfo">Information needed to establish a connection with the database</param>
        /// <returns>Valid Npgsql connection string</returns>
        protected override string ConstructConnectionString()
        {
            return string.Format("Host={0};Port={1};Database={2};Username={3};Password={4}",
                ConnectionInfo.HostName,
                ConnectionInfo.HostPort,
                ConnectionInfo.DatabaseName,
                ConnectionInfo.UserName,
                ConnectionInfo.UserPassword);
        }

        /// <summary>
        /// Attempt to create a new database or do nothing if one with the same
        /// name already exists
        /// </summary>
        protected override void Create()
        {
            try
            {
                string sqlCommand = string.Format(
                    "CREATE TABLE IF NOT EXISTS {0} (" +
                    "   url TEXT PRIMARY KEY NOT NULL," +
                    "   title TEXT NOT NULL," +
                    "   description TEXT NOT NULL," +
                    "   rank REAL NOT NULL," +
                    "   timestamp TIMESTAMP DEFAULT 'epoch'" +
                    ");", ConnectionInfo.TableName);

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
                Console.WriteLine("Error while trying to create tables:\t" + e.Message);
            }
        }

        /// <summary>
        /// Create and initialize a new PostgreSQL database
        /// </summary>
        /// <param name="connectionInfo">Information needed to connect to the database</param>
        public PostgreSQLDatabase(DatabaseConnectionInfo connectionInfo) : base(connectionInfo)
        {}

        /// <summary>
        /// Insert a new crawled page into the database or update an existing entry
        /// </summary>
        /// <param name="pageInfo">Information of the page to add</param>
        /// <param name="tableName">Name of the table to insert the page information into</param>
        /// <returns>True when the operation completed successfully, false otherwise</returns>
        public override bool AddOrUpdateCrawledPage(PageInfo pageInfo, string tableName)
        {
            bool success = true;

            try
            {
                string sqlCommand = string.Format(
                    "INSERT INTO {0} (url, title, description, rank, timestamp)" +
                    "VALUES (@pageUrl, @pageTitle, @pageDescription, @pageRank, TO_TIMESTAMP(@pageTimestamp))" +
                    "ON CONFLICT (url)" +
                    "   DO UPDATE SET url = @pageUrl;", tableName);

                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlCommand, connection))
                    {
                        command.Parameters.AddWithValue("pageUrl", pageInfo.Uri.ToString());
                        command.Parameters.AddWithValue("pageTitle", pageInfo.Title);
                        command.Parameters.AddWithValue("pageDescription", pageInfo.Description);
                        command.Parameters.AddWithValue("pageRank", 0.0f);
                        command.Parameters.AddWithValue("pageTimestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                        command.ExecuteNonQuery();
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

        /// <summary>
        /// Add a new page to the database table with URLs that are pending a crawl
        /// </summary>
        /// <param name="urls">URLs to store</param>
        /// <param name="tableName">Name of the table that contains pending URLs</param>
        /// <returns>True when the operation completed successfully, false otherwise</returns>
        public override bool TryAddPendingUrls(string[] urls, string tableName)
        {
            bool success = true;

            try
            {
                StringBuilder sqlCommandBuilder = new StringBuilder(string.Format("INSERT INTO {0} (url)", tableName));
                sqlCommandBuilder.Append("VALUES ");

                int index = 0;
                foreach (string url in urls)
                {
                    sqlCommandBuilder.Append("(\'");
                    sqlCommandBuilder.Append(url);
                    sqlCommandBuilder.Append("\')");

                    if (index++ < urls.Length - 1)
                    {
                        sqlCommandBuilder.Append(',');
                    }
                }

                sqlCommandBuilder.Append("ON CONFLICT (url)");
                sqlCommandBuilder.Append("DO NOTHING;");

                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlCommandBuilder.ToString(), connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while adding pending URL:\t" + e.Message);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Retrieve URLs that have not been crawled yet
        /// </summary>
        /// <param name="count">Maximum number of URLs to retrieve</param>
        /// <param name="tableName">Name of the table to retrieve the URLs from</param>
        /// <returns>Array of URLs</returns>
        public override string[] GetUncrawledUrls(int count, string tableName)
        {
            List<string> returnUrls = new List<string>();

            try
            {
                string sqlSelectCommand = string.Format(
                    "SELECT url\n" +
                    "FROM {0}\n" +
                    "LIMIT @count;", tableName);

                using (NpgsqlConnection connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlSelectCommand, connection))
                    {
                        command.Parameters.AddWithValue("count", count);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                returnUrls.Add(reader["url"].ToString());
                            }
                        }
                    }

                    if (returnUrls.Count > 0)
                    {
                        // Construct and execute a command to delete the previously
                        // retrieved URLs
                        string sqlDeleteCommand = string.Format(
                            "DELETE FROM {0}\n" +
                            "WHERE url IN (", tableName);

                        int index = 0;
                        foreach (string url in returnUrls)
                        {
                            sqlDeleteCommand += ('\'' + url + '\'');

                            if (index < returnUrls.Count - 1)
                            {
                                sqlDeleteCommand += ',';
                            }
                        }

                        sqlDeleteCommand += ");";

                        using (NpgsqlCommand command = new NpgsqlCommand(sqlDeleteCommand, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while attempting to retrieve URLs to crawl:\t" + e.Message);
            }

            return returnUrls.ToArray();
        }
    }
}
