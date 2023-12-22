using System.Xml.Linq;
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

            try
            {
                string query = "select 'TABLE' as ObjectType, [object_id],[name] from sys.tables where [name] <> '__RefactorLog' union select 'VIEW',[object_id],[name] from sys.views order by[name] asc";
                using (var db = context.getConnection())
                using (var cmd = db.CreateCommand())
                {
                    var dt = Entity.Run.On(cmd).Query(query).ToList();
                    result = EntityHelper.ColumnToEntities<DbEntity>(dt);
                }
            }
            catch
            {
                result = new List<DbEntity>();
            }

            return result;
        }

        public List<DbEntity> GetSpEntities()
        {
            var result = new List<DbEntity>();

            try
            {
                string query = "select 'SP' as ObjectType, [object_id], [name] from sys.procedures order by [name] asc";
                using (var db = context.getConnection())
                using (var cmd = db.CreateCommand())
                {
                    var dt = Entity.Run.On(cmd).Query(query).ToList();
                    result = EntityHelper.ColumnToEntities<DbEntity>(dt);
                }
            }
            catch
            {
                result = new List<DbEntity>();
            }

            return result;
        }

        public List<DbTableInfo> GetTableProperties(string tableName)
        {
            var result = new List<DbTableInfo>();

            try
            {
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
,	A.Mode
from (select [object_id] as TableID,[name] as TableName,'TABLE' as [Mode] from sys.tables union select [object_id],[name],'VIEW' from sys.views) as A
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
            }
            catch
            {
                result = new List<DbTableInfo>();
            }

            return result;
        }

        public List<SPEntity> GetSpProperties(string spName)
        {
            var result = new List<SPEntity>();

            try
            {
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
            }
            catch
            {
                result = new List<SPEntity>();
            }

            return result;
        }

        public void ClearDatabase()
        {
            string query = @$"
-- 모든 뷰 삭제
DECLARE @viewName NVARCHAR(MAX);
DECLARE viewCursor CURSOR FOR
SELECT name
FROM sys.views; -- 모든 뷰를 선택

OPEN viewCursor;
FETCH NEXT FROM viewCursor INTO @viewName;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('DROP VIEW ' + @viewName);
    FETCH NEXT FROM viewCursor INTO @viewName;
END;

CLOSE viewCursor;
DEALLOCATE viewCursor;

-- 모든 저장 프로시저(SP) 삭제
DECLARE @spName NVARCHAR(MAX);
DECLARE spCursor CURSOR FOR
SELECT name
FROM sys.objects
WHERE type = 'P'; -- 모든 저장 프로시저를 선택

OPEN spCursor;
FETCH NEXT FROM spCursor INTO @spName;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('DROP PROCEDURE ' + @spName);
    FETCH NEXT FROM spCursor INTO @spName;
END;

CLOSE spCursor;
DEALLOCATE spCursor;

-- 모든 테이블 반환 함수 삭제
DECLARE @tableFunctionName NVARCHAR(MAX);
DECLARE tableFunctionCursor CURSOR FOR
SELECT name
FROM sys.objects
WHERE type = 'TF'; -- 모든 테이블 반환 함수를 선택

OPEN tableFunctionCursor;
FETCH NEXT FROM tableFunctionCursor INTO @tableFunctionName;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('DROP FUNCTION ' + @tableFunctionName);
    FETCH NEXT FROM tableFunctionCursor INTO @tableFunctionName;
END;

CLOSE tableFunctionCursor;
DEALLOCATE tableFunctionCursor;

-- 모든 스칼라 반환 함수 삭제
DECLARE @scalarFunctionName NVARCHAR(MAX);
DECLARE scalarFunctionCursor CURSOR FOR
SELECT name
FROM sys.objects
WHERE type = 'FN'; -- 모든 스칼라 반환 함수를 선택

OPEN scalarFunctionCursor;
FETCH NEXT FROM scalarFunctionCursor INTO @scalarFunctionName;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('DROP FUNCTION ' + @scalarFunctionName);
    FETCH NEXT FROM scalarFunctionCursor INTO @scalarFunctionName;
END;

CLOSE scalarFunctionCursor;
DEALLOCATE scalarFunctionCursor;

-- 모든 테이블 삭제
DECLARE @tableName NVARCHAR(MAX);
DECLARE tableCursor CURSOR FOR
SELECT name
FROM sys.objects
WHERE type = 'U'; -- 모든 테이블을 선택

OPEN tableCursor;
FETCH NEXT FROM tableCursor INTO @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('DROP TABLE ' + @tableName);
    FETCH NEXT FROM tableCursor INTO @tableName;
END;

CLOSE tableCursor;
DEALLOCATE tableCursor;
";

            try
            {
                using (var db = context.getConnection())
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 1000 * 60 * 2;
                    cmd.ExecuteNonQuery();

                }
            }
            catch
            {

            }
        }

        public List<SpTable> GetSPTables(string spName)
        {
            var result = new List<SpTable>();

            try
            {
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
            }
            catch
            {
                result = new List<SpTable>();
            }

            return result;
        }

        public List<SpOutput> GetSpOutput(string spName)
        {
            var result = new List<SpOutput>();

            try
            {
                string query = @$"EXEC sp_describe_first_result_set N'{spName}', null, 0;";
                using (var db = context.getConnection())
                using (var cmd = db.CreateCommand())
                {
                    var dt = Entity.Run.On(cmd).Query(query).ToList();
                    result = EntityHelper.ColumnToEntities<SpOutput>(dt);
                }
            }
            catch
            {
                result = new List<SpOutput>();
            }

            return result;
        }

        public List<ForeignKeyInfo> GetForeignKeys()
        {
            var result = new List<ForeignKeyInfo>();

            try
            {
                string query = @$"SELECT 
    FK.name AS ForeignKeyName,
    TP.name AS ParentTableName,
    CP.name AS ParentColumnName,
    RFK.name AS ReferencedTableName,
    RC.name AS ReferencedColumnName
FROM 
    sys.foreign_keys AS FK
INNER JOIN 
    sys.tables AS TP ON FK.parent_object_id = TP.object_id
INNER JOIN 
    sys.tables AS RFK ON FK.referenced_object_id = RFK.object_id
INNER JOIN 
    sys.foreign_key_columns AS FKC ON FKC.constraint_object_id = FK.object_id
INNER JOIN 
    sys.columns AS CP ON FKC.parent_column_id = CP.column_id AND FKC.parent_object_id = CP.object_id
INNER JOIN 
    sys.columns AS RC ON FKC.referenced_column_id = RC.column_id AND FKC.referenced_object_id = RC.object_id;";
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

        ~SqlServerRepository()
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
