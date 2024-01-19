using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Woose.Builder
{
    public class NestJsHelper
    {
        public NestJsHelper()
        {
        }

        public string DtoCreate(BindOption option, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string entityName = info[0].TableName;

                builder.AppendLine("import { ApiProperty } from '@nestjs/swagger';");
                builder.AppendLine("import { " + entityName + " } from 'src/entities';");
                builder.AppendEmptyLine();
                builder.AppendLine("class " + entityName.FirstCharToUpper() + "Regist {");
                foreach (var item in info)
                {
                    if (!item.is_identity && !item.IsDate)
                    {
                        builder.AppendTabLine(1, "@ApiProperty({ description: '" + item.Name + "' })");
                        builder.AppendTabLine(1, $"public {item.Name.FirstCharToLower()}:{item.ScriptType};");
                        builder.AppendEmptyLine();
                    }
                }
                builder.AppendTabLine(1, "constructor() {");
                foreach (var item in info)
                {
                    if (!item.is_identity && !item.IsDate)
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
                }
                builder.AppendTabLine(1, "}");
                builder.AppendLine("}");
                builder.AppendEmptyLine();
                builder.AppendLine("class " + entityName.FirstCharToUpper() + "Update {");
                foreach (var item in info)
                {
                    if (!item.IsDate)
                    {
                        builder.AppendTabLine(1, "@ApiProperty({ description: '" + item.Name + "' })");
                        builder.AppendTabLine(1, $"public {item.Name.FirstCharToLower()}:{item.ScriptType};");
                        builder.AppendEmptyLine();
                    }
                }
                builder.AppendTabLine(1, "constructor() {");
                foreach (var item in info)
                {
                    if (!item.IsDate)
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
                }
                builder.AppendTabLine(1, "}");
                builder.AppendLine("}");
                builder.AppendEmptyLine();
                builder.AppendLine("export { " + $"{entityName.FirstCharToUpper()}Regist, {entityName.FirstCharToUpper()}Update" + " }");
            }

            return builder.ToString();
        }


        public string EntitiyCreate(BindOption option, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string entityName = info[0].TableName;

                builder.AppendLine("import { ApiProperty } from '@nestjs/swagger';");
                builder.AppendLine("import { Entity,Column,PrimaryGeneratedColumn,CreateDateColumn,UpdateDateColumn } from 'typeorm';");
                builder.AppendEmptyLine();
                builder.AppendLine("@Entity({name: '" + entityName + "' })");
                builder.AppendLine("export class " + entityName.FirstCharToUpper() + " {");
                foreach (var item in info)
                {
                    if (item.is_identity)
                    {
                        builder.AppendTabLine(1, "@PrimaryGeneratedColumn({ name: '" + item.Name + "', type : '" + item.ColumnType + "' })");
                    }
                    else
                    {
                        if (item.IsDate)
                        {
                            if (item.Name.IndexOf("regist", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                builder.AppendTabLine(1, "@CreateDateColumn({ type: 'datetime2', name: '" + item.Name + "' })");
                            }
                            else if (item.Name.IndexOf("update", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                builder.AppendTabLine(1, "@UpdateDateColumn({ type: 'datetime2', name: '" + item.Name + "' })");
                            }
                            else
                            {
                                builder.AppendTabLine(1, "@ApiProperty({ description: '" + item.Name + "' })");
                                builder.AppendTab(1, "@Column('" + item.ColumnType + "', { name: '" + item.Name + "'");
                                if (item.IsSize)
                                {
                                    builder.Append(", length : " + Convert.ToString(item.max_length));
                                }
                                builder.AppendLine(" })");
                            }
                        }
                        else
                        {
                            builder.AppendTabLine(1, "@ApiProperty({ description: '" + item.Name + "' })");
                            builder.AppendTab(1, "@Column('" + item.ColumnType + "', { name: '" + item.Name + "'");
                            if (item.IsSize)
                            {
                                builder.Append(", length : " + Convert.ToString(item.max_length));
                            }
                            builder.AppendLine(" })");
                        }
                    }
                    builder.AppendTabLine(1, $"{item.Name.FirstCharToLower()}: {item.ScriptType};");
                    builder.AppendEmptyLine();
                }
                builder.AppendLine("}");
            }

            return builder.ToString();
        }
    }
}
