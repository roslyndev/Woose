namespace Woose.Builder
{
    public class SpOutput
    {
        public int column_ordinal { get; set; } = 0;

        public string name { get; set; } = string.Empty;

        public string system_type_name { get; set; } = string.Empty;

        public SpOutput()
        {
        }

        public string CsType
        {
            get
            {
                switch (this.system_type_name)
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
                switch (this.system_type_name)
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
                switch (this.system_type_name)
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
