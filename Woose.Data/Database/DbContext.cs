using System.Data.SqlClient;

namespace Woose.Data
{
    public class DbContext : IContext
    {
        private readonly string ConnectionString;

        public DbContext(string connectionString) 
        { 
            this.ConnectionString = connectionString;
        }

        public string GetConnectionString
        {
            get
            {
                return this.ConnectionString;
            }
        }

        public SqlConnection getConnection()
        {
            var sqlconn = new SqlConnection(this.ConnectionString);
            sqlconn.Open();
            return sqlconn;
        }

        public static SqlConnection CreateConnection(string connectionstring)
        {
            var sqlconn = new SqlConnection(connectionstring);
            sqlconn.Open();
            return sqlconn;
        }
    }

    public static class ExtendDbContext
    {
        public static SqlCommand CreateCommand(this SqlConnection conn)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            return cmd;
        }
    }
}
