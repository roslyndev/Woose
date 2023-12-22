using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Woose.Builder
{
    public class TypeScriptCreater
    {
        public TypeScriptCreater()
        {
        }

        public string CreateEntity(BindOption options, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                builder.Append($"export class {info[0].TableName} ");
                builder.AppendLine("{");
                foreach (var item in info)
                {
                    builder.AppendTabLine(1, $"public {item.Name.FirstCharToLower()}:{item.ScriptType};");
                }
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "constructor() {");
                foreach (var item in info)
                {
                    builder.AppendTab(2, $"this.{item.Name.FirstCharToLower()} = ");
                    switch (item.ScriptType)
                    {
                        case "string":
                            builder.AppendLine("\"\";");
                            break;
                        case "number":
                            builder.AppendLine("0;");
                            break;
                        case "Date":
                            builder.AppendLine("new Date();");
                            break;
                        case "boolean":
                            builder.AppendLine("false;");
                            break;
                        default:
                            builder.AppendLine("null;");
                            break;
                    }
                }
                builder.AppendTabLine(1, "}");
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        public string CreateSpEntity(OptionData options, List<SPEntity> properties, List<SpTable> tables, List<SpOutput> outputs)
        {
            StringBuilder builder = new StringBuilder(200);

            string returnModel = string.Empty;
            string funcName = string.Empty;
            DbTypeItem typeObj = null;

            if (tables != null && tables.Count > 0)
            {
                funcName = CodeHelper.GetNameFromSP(tables[0].name);
            }
            else
            {
                funcName = "CustomSp";
            }

            if (outputs != null && outputs.Count > 0)
            {
                builder.AppendLine($"//반환 파라미터 모델");
                builder.AppendLine($"export class {funcName}");
                builder.AppendLine("{");
                
                foreach (var item in outputs)
                {
                    typeObj = DbTypeHelper.MSSQL.ParseColumnType(item.system_type_name);
                    builder.AppendTabLine(1, $"public {item.name.FirstCharToLower()}:{DbTypeHelper.MSSQL.GetObjectTypeByTypeScript(typeObj.Name)};");
                }
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, $"constructor()");
                builder.AppendTabLine(1, "{");
                foreach (var item in outputs)
                {
                    typeObj = DbTypeHelper.MSSQL.ParseColumnType(item.system_type_name);
                    builder.AppendTabLine(2, $"this.{item.name.FirstCharToLower()} = {DbTypeHelper.MSSQL.GetObjectDefaultValueByTypeScript(typeObj.Name)};");
                }
                builder.AppendTabLine(1, "}");
                builder.AppendLine("}");
                builder.AppendEmptyLine();
            }

            if (properties != null && properties.Count > 0)
            {
                builder.AppendLine($"//입력 파라미터 모델");
                builder.AppendLine($"export class Input{funcName}");
                builder.AppendLine("{");
                foreach (var item in properties.Where(x => !x.is_output))
                {
                    builder.AppendTabLine(1, $"public {item.name.Replace("@", "").FirstCharToLower()}:{DbTypeHelper.MSSQL.GetObjectTypeByTypeScript(item.type)};");
                }
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, $"constructor()");
                builder.AppendTabLine(1, "{");
                foreach (var item in properties.Where(x => !x.is_output))
                {
                    builder.AppendTabLine(2, $"this.{item.name.Replace("@", "").FirstCharToLower()} = {DbTypeHelper.MSSQL.GetObjectDefaultValueByTypeScript(item.type)};");
                }
                builder.AppendTabLine(1, "}");
                builder.AppendLine("}");
                builder.AppendEmptyLine();
            }

            return builder.ToString();
        }
    }
}
