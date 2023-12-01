using System;
using System.Text.RegularExpressions;

namespace Woose.Builder
{
    public class DbTypeHelper
    {
        private static readonly Lazy<DbTypeHelper> a = new Lazy<DbTypeHelper>(() => new DbTypeHelper());
        public static DbTypeHelper MSSQL { get { return a.Value; } }

        public DbTypeHelper()
        {
        }

        public DbTypeItem ParseColumnType(string columnType)
        {
            var result = new DbTypeItem();
            var match = Regex.Match(columnType, @"(\w+)(\((\d+)\))?");

            if (match.Success)
            {
                int size = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : -1;
                return new DbTypeItem(match.Groups[1].Value, size);
            }

            return new DbTypeItem(columnType, -1);
        }

        public string GetObjectTypeByCsharp(string typeName)
        {
            switch (typeName)
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

        public string GetObjectDefaultValueByCsharp(string typeName)
        {
            switch (typeName)
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
                    return "0";
                case "bigint":
                    return "-1";
                case "float":
                    return "0.0";
                case "timestamp":
                case "date":
                case "time":
                case "datetime2":
                case "datetimeoffset":
                case "smalldatetime":
                case "datetime":
                    return "default!";
                case "text":
                case "ntext":
                case "nvarchar":
                case "nchar":
                case "varchar":
                case "char":
                    return "string.Empty";
                case "bit":
                    return "false";
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
                    return "default!";
            }
        }

        public string GetObjectTypeByTypeScript(string typeName)
        {
            switch (typeName)
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

        public string GetObjectDefaultValueByTypeScript(string typeName)
        {
            switch (typeName)
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
                    return "0";
                case "bigint":
                    return "-1";
                case "float":
                    return "0.0";
                case "timestamp":
                case "date":
                case "time":
                case "datetime2":
                case "datetimeoffset":
                case "smalldatetime":
                case "datetime":
                    return "new Date()";
                case "text":
                case "ntext":
                case "nvarchar":
                case "nchar":
                case "varchar":
                case "char":
                    return "''";
                case "bit":
                    return "false";
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
                    return "null";
            }
        }

        public string GetObjectTypeByYAML(string typeName)
        {
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                if (typeName.IndexOf("(") > -1)
                {
                    typeName = typeName.Substring(0, typeName.IndexOf("("));
                }
            }

            switch (typeName)
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

    public class DbTypeItem
    {
        public string Name { get; set; } = string.Empty;
        public int Size { get; set; } = -1;

        public DbTypeItem()
        {
        }

        public DbTypeItem(string name)
        {
            this.Name = name;
        }

        public DbTypeItem(string name, int size)
        {
            this.Name = name;
            this.Size = size;
        }
    }
}
