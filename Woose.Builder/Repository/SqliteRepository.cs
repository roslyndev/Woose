using Microsoft.Data.Sqlite;
using System.Text;
using Woose.Core;

namespace Woose.Builder
{
    public class SqliteRepository : IDisposable
    {
        private bool disposedValue;

        private const string DbConnectionString = "Data Source=woose.db;";

        protected SqliteConnection SqlConn { get; set; }


        public SqliteRepository()
        {
            this.SqlConn = new SqliteConnection(DbConnectionString);
            this.SqlConn.Open();
        }

        public void Init()
        {
            StringBuilder query = new StringBuilder(200);
            query.AppendLine("CREATE TABLE IF NOT EXISTS Databases (");
            query.AppendLine("Id INTEGER PRIMARY KEY AUTOINCREMENT,");
            query.AppendLine("DatabaseName TEXT NOT NULL,");
            query.AppendLine("DatabaseType TEXT NOT NULL,");
            query.AppendLine("ConnectionString TEXT NOT NULL");
            query.AppendLine(");");
            query.AppendLine("");
            using (var command = new SqliteCommand(query.ToString(), this.SqlConn))
            {
                command.ExecuteNonQuery();
            }
        }

        public ReturnValue InsertDatabase(Database db)
        {
            var result = new ReturnValue();

            result.Value = "Insert";
            string query = "INSERT INTO Databases (DatabaseName, DatabaseType, ConnectionString) VALUES (@DatabaseName, @DatabaseType, @ConnectionString)";

            try
            {
                using (var command = new SqliteCommand(query, this.SqlConn))
                {
                    command.Parameters.AddWithValue("@DatabaseName", db.DatabaseName);
                    command.Parameters.AddWithValue("@DatabaseType", db.DatabaseType);
                    command.Parameters.AddWithValue("@ConnectionString", db.ConnectionString);
                    command.ExecuteNonQuery();
                    result.Check = true;
                    result.Code = 1;
                }
            }
            catch (Exception ex)
            {
                result.Error(ex);
            }
                
            return result;
        }

        public ReturnValue UpdateDatabase(Database db)
        {
            var result = new ReturnValue();

            if (db.Id > 0)
            {
                result.Value = "Update";
                string query = $"Update Databases set DatabaseName = @DatabaseName, DatabaseType = @DatabaseType, ConnectionString = @ConnectionString where Id = {db.Id}";

                try
                {
                    using (var command = new SqliteCommand(query, this.SqlConn))
                    {
                        command.Parameters.AddWithValue("@DatabaseName", db.DatabaseName);
                        command.Parameters.AddWithValue("@DatabaseType", db.DatabaseType);
                        command.Parameters.AddWithValue("@ConnectionString", db.ConnectionString);
                        command.ExecuteNonQuery();
                        result.Check = true;
                        result.Code = db.Id;
                    }
                }
                catch (Exception ex)
                {
                    result.Error(ex);
                }
            }
            else
            {
                result.Error("Id값이 없습니다.");
            }

            return result;
        }

        public ReturnValue Deleteatabase(Database db)
        {
            var result = new ReturnValue();

            if (db.Id > 0)
            {
                result.Value = "Delete";
                string query = $"Delete from Databases where Id = {db.Id}";

                try
                {
                    using (var command = new SqliteCommand(query, this.SqlConn))
                    {
                        command.ExecuteNonQuery();
                        result.Check = true;
                        result.Code = db.Id;
                    }
                }
                catch (Exception ex)
                {
                    result.Error(ex);
                }
            }
            else
            {
                result.Error("Id값이 없습니다.");
            }

            return result;
        }

        public Database GetName(string name)
        {
            string selectQuery = "SELECT * FROM Databases WHERE DatabaseName = @Name";
            using (SqliteCommand cmd = new SqliteCommand(selectQuery, this.SqlConn))
            {
                cmd.Parameters.AddWithValue("@Name", name);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Database
                        {
                            Id = reader.GetInt32(0),
                            DatabaseName = reader["DatabaseName"].ToString(),
                            DatabaseType = reader["DatabaseType"].ToString(),
                            ConnectionString = reader["ConnectionString"].ToString()
                        };
                    }
                }
            }

            return null; // 해당 이름의 데이터를 찾지 못한 경우
        }

        public ReturnValues<List<Database>> GetDatabases()
        {
            var result = new ReturnValues<List<Database>>();
            List<Database> databases = new List<Database>();

            string query = "SELECT * FROM Databases";
            try
            {
                using (var command = new SqliteCommand(query, this.SqlConn))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Database database = new Database
                        {
                            Id = reader.GetInt32(0),
                            DatabaseName = reader["DatabaseName"].ToString(),
                            DatabaseType = reader["DatabaseType"].ToString(),
                            ConnectionString = reader["ConnectionString"].ToString()
                        };

                        databases.Add(database);
                    }
                }

                result.Success(databases.Count, databases);
            }
            catch (Exception ex)
            {
                result.Error(ex);
            }

            return result;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.SqlConn != null)
                    {
                        if (this.SqlConn.State == System.Data.ConnectionState.Open)
                        {
                            this.SqlConn.Close();
                        }
                        this.SqlConn.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        ~SqliteRepository()
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
