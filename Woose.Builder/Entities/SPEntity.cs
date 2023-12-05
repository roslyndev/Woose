using System.Data;

namespace Woose.Builder
{
    public class SPEntity
    {
        public string name { get; set; } = string.Empty;

        public string Name
        {
            get
            {
                return this.name.Replace("@", "");
            }
        }

        public string type { get; set; } = string.Empty;

        public int max_length { get; set; } = -1;

        public bool is_output { get; set; } = false;

        public bool has_default_value { get; set; } = false;

        public bool is_nullable { get; set; } = false;

        public string SPName { get; set; } = string.Empty;

        public SPEntity()
        {
        }

        public string DbTypeString
        {
            get
            {
                return this.DbType.ToString();
            }
        }

        public string ScriptType
        {
            get
            {
                switch (this.type)
                {
                    case "tinyint":
                    case "smallint":
                    case "int":
                    case "real":
                    case "money":
                    case "decimal":
                    case "numeric":
                    case "smallmoney":
                    case "uniqueidentifier":
                    case "bigint":
                    case "float":
                        return "number";
                    case "timestamp":
                    case "date":
                    case "time":
                    case "datetime2":
                    case "datetimeoffset":
                    case "smalldatetime":
                    case "datetime":
                        return "Date";
                    case "text":
                    case "ntext":
                    case "nvarchar":
                    case "nchar":
                    case "varchar":
                    case "char":
                        return "string";
                    case "bit":
                        return "boolean";
                    case "sql_variant":
                    case "hierarchyid":
                    case "geometry":
                    case "geography":
                    case "varbinary":
                    case "binary":
                    case "xml":
                    case "sysname":
                    case "image":
                    default:
                        return "object";
                }
            }
        }

        public SqlDbType DbType
        {
            get
            {
                switch (this.type)
                {
                    case "tinyint":
                        return SqlDbType.TinyInt;
                    case "smallint":
                        return SqlDbType.SmallInt;
                    case "int":
                        return SqlDbType.Int;
                    case "real":
                        return SqlDbType.Real;
                    case "money":
                        return SqlDbType.Money;
                    case "smallmoney":
                        return SqlDbType.SmallMoney;
                    case "decimal":
                    case "numeric":
                        return SqlDbType.Decimal;
                    case "bigint":
                        return SqlDbType.BigInt;
                    case "uniqueidentifier":
                        return SqlDbType.UniqueIdentifier;
                    case "float":
                        return SqlDbType.Float;
                    case "timestamp":
                        return SqlDbType.Timestamp;
                    case "date":
                        return SqlDbType.Date;
                    case "time":
                        return SqlDbType.Time;
                    case "datetimeoffset":
                        return SqlDbType.DateTimeOffset;
                    case "smalldatetime":
                        return SqlDbType.SmallDateTime;
                    case "datetime":
                        return SqlDbType.DateTime;
                    case "datetime2":
                        return SqlDbType.DateTime2;
                    case "text":
                        return SqlDbType.Text;
                    case "ntext":
                        return SqlDbType.NText;
                    case "nvarchar":
                        return SqlDbType.NVarChar;
                    case "nchar":
                        return SqlDbType.NChar;
                    case "char":
                        return SqlDbType.Char;
                    case "bit":
                        return SqlDbType.Bit;
                    case "sql_variant":
                        return SqlDbType.Variant;
                    case "hierarchyid":
                    case "geometry":
                    case "geography":
                    case "varbinary":
                        return SqlDbType.VarBinary;
                    case "binary":
                        return SqlDbType.Binary;
                    case "xml":
                        return SqlDbType.Xml;
                    case "image":
                        return SqlDbType.Image;
                    case "sysname":
                    case "varchar":
                    default:
                        return SqlDbType.VarChar;
                }
            }
        }

        public string CsType
        {
            get
            {
                switch (this.type)
                {
                    case "tinyint":
                        return "TinyInt";
                    case "smallint":
                        return "SmallInt";
                    case "int":
                        return "Int";
                    case "real":
                        return "Real";
                    case "money":
                        return "Money";
                    case "decimal":
                    case "numeric":
                        return "Decimal";
                    case "smallmoney":
                        return "SmallMoney";
                    case "uniqueidentifier":
                        return "UniqueIdentifier";
                    case "bigint":
                        return "BigInt";
                    case "float":
                        return "Float";
                    case "timestamp":
                        return "Timestamp";
                    case "date":
                        return "Date";
                    case "time":
                        return "Time";
                    case "datetime2":
                        return "DateTime2";
                    case "datetimeoffset":
                        return "DateTimeOffset";
                    case "smalldatetime":
                        return "SmallDateTime";
                    case "datetime":
                        return "DateTime";
                    case "text":
                        return "Text";
                    case "ntext":
                        return "NText";
                    case "nvarchar":
                        return "NVarChar";
                    case "nchar":
                        return "NChar";
                    case "varchar":
                        return "VarChar";
                    case "char":
                        return "Char";
                    case "bit":
                        return "Bit";
                    case "varbinary":
                        return "VarBinary";
                    case "binary":
                        return "Binary";
                    case "xml":
                        return "Xml";
                    case "image":
                        return "Image";
                    default:
                        return "Variant";
                }
            }
        }

        public bool IsSize
        {
            get
            {
                switch (this.type)
                {
                    case "text":
                    case "ntext":
                    case "nvarchar":
                    case "nchar":
                    case "varchar":
                    case "char":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public string ObjectType
        {
            get
            {
                switch (this.type)
                {
                    case "tinyint":
                    case "smallint":
                    case "int":
                    case "real":
                    case "money":
                    case "decimal":
                    case "numeric":
                    case "smallmoney":
                    case "uniqueidentifier":
                        return "int";
                    case "bigint":
                        return "long";
                    case "float":
                        return "double";
                    case "timestamp":
                    case "date":
                    case "time":
                    case "datetime2":
                    case "datetimeoffset":
                    case "smalldatetime":
                    case "datetime":
                        return "DateTime";
                    case "text":
                    case "ntext":
                    case "nvarchar":
                    case "nchar":
                    case "varchar":
                    case "char":
                        return "string";
                    case "bit":
                        return "bool";
                    case "sql_variant":
                    case "hierarchyid":
                    case "geometry":
                    case "geography":
                    case "varbinary":
                    case "binary":
                    case "xml":
                    case "sysname":
                    case "image":
                    default:
                        return "object";
                }
            }
        }

    }
}
