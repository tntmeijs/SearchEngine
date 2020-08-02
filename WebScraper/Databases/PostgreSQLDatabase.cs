using Npgsql;

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
    }
}
