namespace Woose.Builder
{
    public class Database
    {
        public int Id { get; set; } = -1;

        public string DatabaseName { get; set; } = string.Empty;

        public string DatabaseType { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty;

        public Database()
        {
        }
    }
}
