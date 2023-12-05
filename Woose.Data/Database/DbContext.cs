using System;
using System.Collections.Generic;
using System.Text;

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

        public DatabaseConnection getConnection()
        {
            return new DatabaseConnection(ConnectionString);
        }

        public static DatabaseConnection CreateConnection(string connectionstring)
        {
            return new DatabaseConnection(connectionstring);
        }

    }
}
