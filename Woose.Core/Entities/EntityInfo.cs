using System.Data;

namespace Woose.Core
{
    public class EntityInfo
    {
        public bool IsKey { get; set; } = false;
        public string ColumnName { get; set; } = string.Empty;
        public SqlDbType Type { get; set; }
        public int Size { get; set; } = -1;

        public enum SizeType
        {
            MaxSize = -1
        }

        public string TypeString
        {
            get
            {
                switch (this.Type)
                {
                    case SqlDbType.VarBinary:
                    case SqlDbType.Variant:
                    case SqlDbType.VarChar:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                    case SqlDbType.Text:
                    case SqlDbType.NText:
                    case SqlDbType.Image:
                        return $"{this.Type.ToString().ToUpper()}({this.Size})";
                    default:
                        return this.Type.ToString().ToUpper();
                }
            }
        }

        public EntityInfo()
        {
        }
    }
}
