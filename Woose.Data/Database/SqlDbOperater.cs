using System.Data.SqlClient;

namespace Woose.Data
{
    public class SqlDbOperater : IDbOperater
    {
        private bool disposedValue;

        protected SqlCommand cmd { get; set; }

        public SqlDbOperater(DatabaseConnection db)
        {
            this.cmd = new SqlCommand();
            this.cmd.Connection = db.GetSqlServer();
        }

        public SqlCommand Command
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
