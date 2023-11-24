namespace Woose.Data.Entities
{
    public class DbEntity
    {
        public string ObjectType { get; set; } = string.Empty;

        public int object_id { get; set; } = -1;

        public string name { get; set; } = string.Empty;

        public DbEntity()
        {
        }

        public string GetQuery()
        {
            return "select 'TABLE' as ObjectType, [object_id],[name] from sys.tables where [name] <> '__RefactorLog' union select 'VIEW',[object_id],[name] from sys.views order by[name] asc";
        }

        public string GetQuery(string tableName)
        {
            return $"select 'TABLE' as ObjectType, [object_id],[name] from sys.tables where [name] <> '__RefactorLog' and [name] = '{tableName}'";
        }
    }
}
