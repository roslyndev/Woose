using System;
using System.Data;

namespace Woose.Builder
{
    public class DbTableInfo
    {
        public string TableID { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;

        public string ColumnName { get; set; } = string.Empty;
        public int column_id { get; set; } = 0;
        public string ColumnType { get; set; } = string.Empty;
        public int ColumnMaxLength { get; set; } = -1;

        public int max_length { get; set; } = -1;

        public bool is_nullable { get; set; } = false;
        public bool is_identity { get; set; } = false;

        public string Description { get; set; } = string.Empty;

        public string Mode { get; set; } = string.Empty;

        public string Name
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Description))
                {
                    return this.ColumnName;
                }
                else
                {
                    return this.Description;
                }
            }
        }

        public bool IsNumber
        {
            get
            {
                switch (this.ColumnType)
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
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsDate
        {
            get
            {
                switch (this.ColumnType)
                {
                    case "timestamp":
                    case "date":
                    case "time":
                    case "datetime2":
                    case "datetimeoffset":
                    case "smalldatetime":
                    case "datetime":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsSize
        {
            get
            {
                switch (this.ColumnType)
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

        public string SequelizeMsSqlType
        {
            get
            {
                switch (this.ColumnType)
                {
                    case "real":
                    case "money":
                    case "decimal":
                    case "numeric":
                    case "smallmoney":
                    case "tinyint":
                    case "smallint":
                    case "int":
                        return "INTEGER";
                    case "uniqueidentifier":
                        return "UUID";
                    case "float":
                        return "FLOAT";
                    case "bigint":
                        return "BIGINT";
                    case "timestamp":
                    case "date":
                    case "time":
                    case "datetime2":
                    case "datetimeoffset":
                    case "smalldatetime":
                    case "datetime":
                        return "DATE";
                    case "text":
                    case "ntext":
                    case "nvarchar":
                    case "nchar":
                    case "varchar":
                    case "char":
                        return "STRING";
                    case "bit":
                        return "BOOLEAN";
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
                        return "Unknown";
                }
            }
        }

        public string ObjectType
        {
            get
            {
                switch (this.ColumnType)
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

        public string CsType
        {
            get
            {
                switch (this.ColumnType)
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

        public string JavaType
        {
            get
            {
                switch (this.ColumnType)
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
                        return "String";
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

        public string ScriptType
        {
            get
            {
                switch (this.ColumnType)
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

        public DbTableInfo() : base()
        {
        }
    }
}
