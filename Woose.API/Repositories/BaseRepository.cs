using System.Data.SqlClient;
using Woose.Core;
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

        public T Single<T>(long idx) where T : IEntity, new()
        {
            T result = new T();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                var info = result.GetInfo().Where(x => x.IsKey).FirstOrDefault();
                if (info != null)
                {
                    cmd.On<T>().Select(1).Where(info.ColumnName, idx).Set();
                    result = cmd.ExecuteEntity<T>();
                }

                if (result == null)
                {
                    result = new T();
                }
            }

            return result;
        }

        public T Single<T>(string whereStr) where T : IEntity, new()
        {
            T result = new T();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<T>().Select(1).Where(whereStr).Set();
                result = cmd.ExecuteEntity<T>();

                if (result == null)
                {
                    result = new T();
                }
            }

            return result;
        }

        public List<T> Select<T>(string whereStr = "") where T : IEntity, new()
        {
            var result = new List<T>();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<T>().Select().Where(whereStr).Set();
                result = cmd.ExecuteEntities<T>();

                if (result == null)
                {
                    result = new List<T>();
                }
            }

            return result;
        }

        public List<T> Select<T>(string whereStr, int TopCount = 10) where T : IEntity, new()
        {
            var result = new List<T>();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<T>().Select(TopCount).Where(whereStr).Set();
                result = cmd.ExecuteEntities<T>();

                if (result == null)
                {
                    result = new List<T>();
                }
            }

            return result;
        }

        public List<T> Select<T>(string whereStr, QueryOption.Sequence OrderBy, int TopCount = 10) where T : IEntity, new()
        {
            var result = new List<T>();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                T target = new T();
                var info = target.GetInfo().Where(x => x.IsKey).FirstOrDefault();
                if (info != null)
                {
                    switch (OrderBy)
                    {
                        case QueryOption.Sequence.ASC:
                            cmd.On<T>().Select(TopCount).Where(whereStr).Asc(info.ColumnName).Set();
                            break;
                        case QueryOption.Sequence.DESC:
                            cmd.On<T>().Select(TopCount).Where(whereStr).Desc(info.ColumnName).Set();
                            break;
                    }

                    result = cmd.ExecuteEntities<T>();
                }

                if (result == null)
                {
                    result = new List<T>();
                }
            }

            return result;
        }

        public List<T> Select<T>(string whereStr, QueryOption.Sequence OrderBy, string OrderColumn, int TopCount = 10) where T : IEntity, new()
        {
            var result = new List<T>();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                switch (OrderBy)
                {
                    case QueryOption.Sequence.ASC:
                        cmd.On<T>().Select(TopCount).Where(whereStr).Asc(OrderColumn).Set();
                        break;
                    case QueryOption.Sequence.DESC:
                        cmd.On<T>().Select(TopCount).Where(whereStr).Desc(OrderColumn).Set();
                        break;
                }

                result = cmd.ExecuteEntities<T>();

                if (result == null)
                {
                    result = new List<T>();
                }
            }

            return result;
        }

        public int Count<T>(string whereStr = "") where T : IEntity, new()
        {
            int result = 0;

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<T>().Count().Where(whereStr).Set();
                result = cmd.ExecuteCount();
            }

            return result;
        }

        public List<T> Paging<T>(IPagingParameter paramData) where T : IEntity, new()
        {
            var result = new List<T>();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                T target = new T();
                var info = target.GetInfo().Where(x => x.IsKey).FirstOrDefault();
                if (info != null)
                {
                    cmd.On<T>().Paging(paramData.PageSize, paramData.CurPage).Where(paramData.ToWhereString()).Desc(info.ColumnName).Set();
                    result = cmd.ExecuteEntities<T>();
                }

                if (result == null)
                {
                    result = new List<T>();
                }
            }

            return result;
        }

        public List<T> Execute<T>(string query) where T : IEntity, new()
        {
            var result = new List<T>();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.Execute(query).Set();
                result = cmd.ExecuteEntities<T>();

                if (result == null)
                {
                    result = new List<T>();
                }
            }

            return result;
        }

        public ReturnValue InsertOut<T>(T target) where T : IEntity, new()
        {
            var result = new ReturnValue();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On(target).Insert().Try().Set();
                result = cmd.ExecuteReturnValue();
            }

            return result;
        }

        public ReturnValue InsertOut<T>(T target, params string[] Columns) where T : IEntity, new()
        {
            var result = new ReturnValue();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On(target).Insert(Columns).Try().Set();
                result = cmd.ExecuteReturnValue();
            }

            return result;
        }

        public ReturnValue UpdateOut<T>(T target) where T : IEntity, new()
        {
            var result = new ReturnValue();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On(target).Update().Try().Set();
                result = cmd.ExecuteReturnValue();
            }

            return result;
        }

        public ReturnValue UpdateOut<T>(T target, params string[] Columns) where T : IEntity, new()
        {
            var result = new ReturnValue();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On(target).Update(Columns).Try().Set();
                result = cmd.ExecuteReturnValue();
            }

            return result;
        }

        public ReturnValue DeleteOut<T>(string whereStr) where T : IEntity, new()
        {
            var result = new ReturnValue();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<T>().Delete().Try().Where(whereStr).Set();
                result = cmd.ExecuteReturnValue();
            }

            return result;
        }


        public ExecuteResult InsertIn<T>(T target) where T : IEntity, new()
        {
            var result = new ExecuteResult();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On(target).Insert().Try().Set();
                result = cmd.ExecuteResult();
            }

            return result;
        }

        public ExecuteResult InsertIn<T>(T target, params string[] Columns) where T : IEntity, new()
        {
            var result = new ExecuteResult();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On(target).Insert(Columns).Try().Set();
                result = cmd.ExecuteResult();
            }

            return result;
        }

        public ExecuteResult UpdateIn<T>(T target) where T : IEntity, new()
        {
            var result = new ExecuteResult();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On(target).Update().Try().Set();
                result = cmd.ExecuteResult();
            }

            return result;
        }

        public ExecuteResult UpdateIn<T>(T target, params string[] Columns) where T : IEntity, new()
        {
            var result = new ExecuteResult();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On(target).Update(Columns).Try().Set();
                result = cmd.ExecuteResult();
            }

            return result;
        }

        public ExecuteResult DeleteIn<T>(string whereStr) where T : IEntity, new()
        {
            var result = new ExecuteResult();

            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                cmd.On<T>().Delete().Try().Where(whereStr).Set();
                result = cmd.ExecuteResult();
            }

            return result;
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
