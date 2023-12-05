using System;
using System.Data.SqlClient;

namespace Woose.Data
{
    public class DatabaseConnection : IDisposable
    {
        private bool disposedValue;

        protected string ConnectionString { get; set; } = string.Empty;

        public DatabaseType DbType { get; set; } = DatabaseType.Unknown;

        protected SqlConnection? SqlConn { get; set; }

        public DatabaseConnection(string connectionstring) 
        { 
            this.ConnectionString = connectionstring;
            this.DbType = GetDatabaseType(this.ConnectionString);

            switch (this.DbType)
            {
                case DatabaseType.SqlServer:
                    this.SqlConn = new SqlConnection(this.ConnectionString);
                    this.SqlConn.Open();
                    break;
                case DatabaseType.MySql:
                    break;
                case DatabaseType.MongoDB:
                    break;
                default:
                    throw new ArgumentException("Unknown Database Type");
            }
        }


        public SqlConnection? GetSqlServer()
        {
            return this.SqlConn;
        }

        public enum DatabaseType
        {
            Unknown,
            SqlServer,
            MySql,
            MongoDB
        }

        public DatabaseType GetDatabaseType(string connectionString)
        {
            if (connectionString.Contains("Initial Catalog", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Database", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseType.SqlServer;
            }
            else if (connectionString.Contains("Server", StringComparison.OrdinalIgnoreCase) &&
                     connectionString.Contains("Uid", StringComparison.OrdinalIgnoreCase) &&
                     connectionString.Contains("Pwd", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseType.MySql;
            }
            else if (connectionString.Contains("mongodb://", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseType.MongoDB;
            }
            else
            {
                // 기타 데이터베이스 유형을 판별할 수 없음
                return DatabaseType.Unknown;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    switch (this.DbType)
                    {
                        case DatabaseType.SqlServer:
                            if (this.SqlConn != null)
                            {
                                if (this.SqlConn.State == System.Data.ConnectionState.Open)
                                {
                                    this.SqlConn.Close();
                                }
                                this.SqlConn.Dispose();
                                this.SqlConn = null;
                            }
                            break;
                        case DatabaseType.MySql:
                            break;
                        case DatabaseType.MongoDB:
                            break;
                    }
                }

                disposedValue = true;
            }
        }

        ~DatabaseConnection()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            //GC.SuppressFinalize(this);
        }
    }
}
