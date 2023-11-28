using System.Data.SqlClient;
using System.Text;

namespace Woose.Data
{
    public class SqlDbOperater : IDbOperater
    {
        private bool disposedValue;

        protected SqlCommand? cmd { get; set; }

        protected StringBuilder query { get; set; } = new StringBuilder(200);

        public string Query
        {
            get
            {
                return query.ToString();
            }
            set
            {
                query = new StringBuilder(value);
                if (this.cmd != null)
                {
                    this.cmd.CommandType = System.Data.CommandType.Text;
                }
            }
        }

        public SqlDbOperater(DatabaseConnection db)
        {
            this.cmd = new SqlCommand();
            this.cmd.Connection = db.GetSqlServer();
        }

        public SqlCommand? Command
        {
            get
            {
                return cmd;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.cmd != null)
                    {
                        this.cmd.Dispose();
                        this.cmd = null;
                    }
                }

                disposedValue = true;
            }
        }

        ~SqlDbOperater()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            //System.GC.SuppressFinalize(this);
        }
    }
}
