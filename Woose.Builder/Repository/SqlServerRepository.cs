using Woose.Core;
using Woose.Data;
using Woose.Data.Entities;

namespace Woose.Builder
{
    public class SqlServerRepository : IDisposable
    {
        private bool disposedValue;

        private DbContext context;

        public SqlServerRepository(DbContext conn)
        {
            this.context = conn;
        }


        public List<DbEntity> GetTableEntities()
        {
            var result = new List<DbEntity>();

            string query = "select 'TABLE' as ObjectType, [object_id],[name] from sys.tables where [name] <> '__RefactorLog' union select 'VIEW',[object_id],[name] from sys.views order by[name] asc";
            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                var dt = Entity.Run.On(cmd).Query(query).ToList();
                result = EntityHelper.ColumnToEntities<DbEntity>(dt);
            }

            return result;
        }

        public List<DbEntity> GetSpEntities()
        {
            var result = new List<DbEntity>();

            string query = "select 'SP' as ObjectType, [object_id], [name] from sys.procedures order by [name] asc";
            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                var dt = Entity.Run.On(cmd).Query(query).ToList();
                result = EntityHelper.ColumnToEntities<DbEntity>(dt);
            }

            return result;
        }

        public List<DbTableInfo> GetTableProperties(string tableName)
        {
            var result = new List<DbTableInfo>();

            string query = @$"select
	A.TableID
,	A.TableName
,	B.[name] as ColumnName
,	B.column_id
,	C.[name] as ColumnType
,	C.max_length as ColumnMaxLength
,	(case when C.[name] = 'nvarchar' or C.[name] = 'nchar' then (B.max_length/2) else B.max_length end) as max_length
,	B.is_nullable
,	B.is_identity
,	D.[value] as [Description]
from (select [object_id] as TableID,[name] as TableName from sys.tables union select [object_id] as TableID,[name] as TableName from sys.views) as A
inner join sys.all_columns as B on A.TableID = B.[object_id]
inner join sys.types as C on B.[system_type_id] = C.[system_type_id] and B.user_type_id = C.user_type_id
left outer join sys.extended_properties as D on D.major_id = B.[object_id] and D.minor_id = B.column_id and D.[name] = 'MS_Description'
where A.TableName = '{tableName}'";
            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                var dt = Entity.Run.On(cmd).Query(query).ToList();
                result = EntityHelper.ColumnToEntities<DbTableInfo>(dt);
            }

            return result;
        }

        public List<SPEntity> GetSpProperties(string spName)
        {
            var result = new List<SPEntity>();

            string query = @$"select
	A.[name]
,	B.[name] as [type]
,	A.max_length
,	A.is_output
,	A.has_default_value
,	A.is_nullable
,	C.[name] as SPName
from sys.parameters as A with (nolock)
inner join sys.types as B with (nolock) on A.system_type_id = B.system_type_id and A.user_type_id = B.user_type_id 
inner join sys.procedures as C with (nolock) on A.[object_id] = C.[object_id] 
where C.[name] = '{spName}'
order by A.parameter_id asc";
            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                var dt = Entity.Run.On(cmd).Query(query).ToList();
                result = EntityHelper.ColumnToEntities<SPEntity>(dt);
            }

            return result;
        }

        public List<SpTable> GetSPTables(string spName)
        {
            var result = new List<SpTable>();

            string query = @$"select DISTINCT A.[name], B.[depid], C.[name] as TableName from SYSOBJECTS as A with(nolock)
inner join SYSDEPENDS as B with (nolock) on A.id = B.id
inner join SYSOBJECTS as C with (nolock) on C.id = B.depid and C.xtype = 'U'
where A.xtype = 'P'
and A.[name] = '{spName}'
order by B.[depid] desc";
            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                var dt = Entity.Run.On(cmd).Query(query).ToList();
                result = EntityHelper.ColumnToEntities<SpTable>(dt);
            }

            return result;
        }

        public List<SpOutput> GetSpOutput(string spName)
        {
            var result = new List<SpOutput>();

            string query = @$"EXEC sp_describe_first_result_set N'{spName}', null, 0;";
            using (var db = context.getConnection())
            using (var cmd = db.CreateCommand())
            {
                var dt = Entity.Run.On(cmd).Query(query).ToList();
                result = EntityHelper.ColumnToEntities<SpOutput>(dt);
            }

            return result;
        }


        public DbEntity? Find(List<DbEntity> tables, List<SpOutput> spoutput)
        {
            if (tables != null && tables.Count > 0)
            {
                int num = 0;
                int i = 0;

                foreach (var table in tables)
                {
                    if (table != null)
                    {
                        try
                        {
                            num = 0;
                            i = 0;
                            var properties = this.GetTableProperties(table.name);
                            if (properties != null)
                            {
                                foreach (var info in properties)
                                {
                                    num += (spoutput.Where(x => x.name.Equals(info.ColumnName, StringComparison.OrdinalIgnoreCase)).Count() > 0) ? 1 : 0;
                                    i++;
                                }

                                if (num == i)
                                {
                                    return table;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return null;
        }

        public DbEntity? Find(List<SpOutput> spoutput)
        {
            List<DbEntity> tables = this.GetTableEntities();

            if (tables != null && tables.Count > 0)
            {
                int num = 0;
                int i = 0;

                foreach (var table in tables)
                {
                    if (table != null)
                    {
                        try
                        {
                            num = 0;
                            i = 0;
                            var properties = this.GetTableProperties(table.name);
                            if (properties != null)
                            {
                                foreach (var info in properties)
                                {
                                    num += (spoutput.Where(x => x.name.Equals(info.ColumnName, StringComparison.OrdinalIgnoreCase)).Count() > 0) ? 1 : 0;
                                    i++;
                                }

                                if (num == i)
                                {
                                    return table;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return null;
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

        ~SqlServerRepository()
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
