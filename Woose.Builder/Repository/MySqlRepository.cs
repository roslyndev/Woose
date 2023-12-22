using System.Xml.Linq;
using Woose.Core;
using Woose.Data;
using Woose.Data.Entities;

namespace Woose.Builder
{
    public class MySqlRepository : IDisposable
    {
        private bool disposedValue;

        private DbContext context;

        public MySqlRepository(DbContext conn)
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
    CONSTRAINT_NAME AS ForeignKeyName,
    TABLE_NAME AS ParentTableName,
    COLUMN_NAME AS ParentColumnName,
    REFERENCED_TABLE_NAME AS ReferencedTableName,
    REFERENCED_COLUMN_NAME AS ReferencedColumnName
FROM 
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE 
    CONSTRAINT_NAME <> 'PRIMARY';";
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

        ~MySqlRepository()
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
