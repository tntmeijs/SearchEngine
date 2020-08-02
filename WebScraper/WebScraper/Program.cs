using Databases;

namespace WebScraper
{
    /// <summary>
    /// Main application
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Database used to store the results of the program in
        /// </summary>
        private Database ProgramDatabase;

        /// <summary>
        /// Initialize the database
        /// </summary>
        /// <param name="type">Type of database to create</param>
        /// <param name="connectionString">URL used to connect to the database</param>
        private void InitializeDatabase(Database.DatabaseType type, string connectionString)
        {
            ProgramDatabase = Database.CreateInstance(type);
            ProgramDatabase.Connect(connectionString);
        }

        /// <summary>
        /// Create a new main program
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        public Program(string[] args)
        {
            InitializeDatabase(Database.DatabaseType.PostgreSQL, "");
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
