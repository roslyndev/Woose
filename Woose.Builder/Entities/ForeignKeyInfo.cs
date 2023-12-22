namespace Woose.Builder
{
    public class ForeignKeyInfo
    {
        public string ForeignKeyName { get; set; } = string.Empty;

        public string ParentTableName { get; set; } = string.Empty;

        public string ParentColumnName { get; set; } = string.Empty;

        public string ReferencedTableName { get; set; } = string.Empty;

        public string ReferencedColumnName { get; set; } = string.Empty;

        public ForeignKeyInfo() 
        { 
        }
    }
}
