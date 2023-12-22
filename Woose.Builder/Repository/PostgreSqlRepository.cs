using System.Xml.Linq;
using Woose.Core;
using Woose.Data;
using Woose.Data.Entities;

namespace Woose.Builder
{
    public class PostgreSqlRepository : IDisposable
    {
        private bool disposedValue;

        private DbContext context;

        public PostgreSqlRepository(DbContext conn)
        {
            this.context = conn;
        }


        public List<DbEntity> GetTableEntities()
        {
            var result = new List<DbEntity>();



            return result;
        }

        public List<DbEntity> GetSpEntities()
        {
            var result = new List<DbEntity>();



            return result;
        }

        public List<DbTableInfo> GetTableProperties(string tableName)
        {
            var result = new List<DbTableInfo>();



            return result;
        }

        public List<SPEntity> GetSpProperties(string spName)
        {
            var result = new List<SPEntity>();



            return result;
        }

        public void ClearDatabase()
        {

        }

        public List<SpTable> GetSPTables(string spName)
        {
            var result = new List<SpTable>();



            return result;
        }

        public List<SpOutput> GetSpOutput(string spName)
        {
            var result = new List<SpOutput>();


            return result;
        }

        public List<ForeignKeyInfo> GetForeignKeys()
        {
            var result = new List<ForeignKeyInfo>();

            try
            {
                string query = @$"SELECT 
    conname AS ForeignKeyName,
    conrelid::regclass AS ParentTableName,
    a.attname AS ParentColumnName,
    confrelid::regclass AS ReferencedTableName,
    a1.attname AS ReferencedColumnName
FROM 
    pg_constraint
JOIN 
    pg_attribute a ON a.attnum = ANY(conkey) AND a.attrelid = conrelid
JOIN 
    pg_attribute a1 ON a1.attnum = ANY(confkey) AND a1.attrelid = confrelid;";
                using (var db = context.getConnection())
                using (var cmd = db.CreateCommand())
                {
                    var dt = Entity.Run.On(cmd).Query(query).ToList();
                    result = EntityHelper.ColumnToEntities<ForeignKeyInfo>(dt);
                }
            }
            catch
            {
                result = new List<ForeignKeyInfo>();
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

        ~PostgreSqlRepository()
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
