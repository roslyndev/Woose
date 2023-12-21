using System.Data.SqlClient;

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

    public static class DatabaseInfo
    {
        public static string GetConnectionString(this Database db)
        {
            string result = string.Empty;

            if (db != null && !string.IsNullOrWhiteSpace(db.ConnectionString))
            {
                switch (db.DatabaseType.Trim())
                {
                    case "MSSQL":
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(db.ConnectionString);
                        result = builder.ConnectionString;
                        break;
                    case "MySQL":
                        break;
                    case "SQLite":
                        break;
                }
            }

            return result;
        }

        public static string GetHost(this Database db)
        {
            string result = string.Empty;

            if (db != null && !string.IsNullOrWhiteSpace(db.ConnectionString))
            {
                switch (db.DatabaseType.Trim())
                {
                    case "SQL Server":
                    case "MSSQL":
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(db.ConnectionString);
                        result = builder.DataSource;
                        break;
                    case "MySQL":
                        break;
                    case "SQLite":
                        break;
                }
            }

            return result;
        }

        public static string GetID(this Database db)
        {
            string result = string.Empty;

            if (db != null && !string.IsNullOrWhiteSpace(db.ConnectionString))
            {
                switch (db.DatabaseType.Trim())
                {
                    case "SQL Server":
                    case "MSSQL":
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(db.ConnectionString);
                        result = builder.UserID;
                        break;
                    case "MySQL":
                        break;
                    case "SQLite":
                        break;
                }
            }

            return result;
        }

        public static string GetDb(this Database db)
        {
            string result = string.Empty;

            if (db != null && !string.IsNullOrWhiteSpace(db.ConnectionString))
            {
                switch (db.DatabaseType.Trim())
                {
                    case "SQL Server":
                    case "MSSQL":
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(db.ConnectionString);
                        result = builder.InitialCatalog;
                        break;
                    case "MySQL":
                        break;
                    case "SQLite":
                        break;
                }
            }

            return result;
        }

        public static string GetPassword(this Database db)
        {
            string result = string.Empty;

            if (db != null && !string.IsNullOrWhiteSpace(db.ConnectionString))
            {
                switch (db.DatabaseType.Trim())
                {
                    case "SQL Server":
                    case "MSSQL":
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(db.ConnectionString);
                        result = builder.Password;
                        break;
                    case "MySQL":
                        break;
                    case "SQLite":
                        break;
                }
            }

            return result;
        }

        public static string GetAppName(this Database db)
        {
            string result = string.Empty;

            if (db != null && !string.IsNullOrWhiteSpace(db.ConnectionString))
            {
                switch (db.DatabaseType.Trim())
                {
                    case "MSSQL":
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(db.ConnectionString);
                        result = builder.ApplicationName;
                        break;
                    case "MySQL":
                        break;
                    case "SQLite":
                        break;
                }
            }

            return result;
        }

        public static int GetPort(this Database db)
        {
            int result = -1;

            if (db != null && !string.IsNullOrWhiteSpace(db.ConnectionString))
            {
                switch (db.DatabaseType.Trim())
                {
                    case "MSSQL":
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(db.ConnectionString);
                        result = 1433;
                        break;
                    case "MySQL":
                        break;
                    case "SQLite":
                        break;
                }
            }

            return result;
        }
    }
}
