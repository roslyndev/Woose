using System.Data.SqlClient;
using Woose.Data;

namespace Woose.API
{
    public class BaseRepository : IRepository, IDisposable
    {
        protected IContext context;

        private bool disposedValue;

        public BaseRepository(IContext _context)
        {
            this.context = _context;
        }

        public SqlTransaction BeginTransaction(SqlConnection conn)
        {
            return conn.BeginTransaction();
        }

        public void Commit(SqlTransaction tran)
        {
            tran.Commit();
        }

        public void Rollback(SqlTransaction tran)
        {
            tran.Rollback();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        ~BaseRepository()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
