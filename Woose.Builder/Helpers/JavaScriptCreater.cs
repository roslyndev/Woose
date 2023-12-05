using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Woose.Builder
{
    public class JavaScriptCreater
    {
        public JavaScriptCreater()
        {
        }

        public string NodeControllerMethodCreate(BindOption options, List<SPEntity> info, List<SpTable> tables)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0 && tables != null && tables.Count > 0)
            {
                string spName = tables[0].name;
                string mainTable = tables[0].TableName;
                string method = spName.Replace("USP_", "").Replace("_", "").Trim();
                int num = 0;

                builder.Append($"exports.{method.FirstCharToLower()} = async (");
                foreach (var item in info)
                {
                    if (num > 0) { builder.Append(", "); }
                    builder.Append($"{item.name.Replace("@","").FirstCharToLower()}");
                    num++;
                }
                builder.AppendLine(") => {");
                builder.AppendTabStringLine(1, "let result = new ApiResult();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "try {");
                builder.AppendTabString(2, $"const [results, metadata] = await sequelize.query('EXEC {spName} ");
                num = 0;
                foreach(var item in info)
                {
                    if (num > 0) { builder.Append(", "); }
                    builder.Append($"{item.name} = :{item.name.Replace("@", "")}");
                    num++;
                }
                builder.AppendLine("', {");
                builder.AppendTabStringLine(3, "replacements: {");
                num = 0;
                foreach (var item in info)
                {
                    if (num > 0) 
                    { 
                        builder.AppendTabString(4, ","); 
                    }
                    else
                    {
                        builder.AppendTabString(4, "");
                    }
                    builder.AppendTabStringLine(1, $"{item.name.Replace("@", "")}: {item.name.Replace("@", "").FirstCharToLower()}");
                    num++;
                }
                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(2, "");
                builder.AppendTabStringLine(3, "if (metadata && metadata.rowsAffected && metadata.rowsAffected[0] > 0) {");
                builder.AppendTabStringLine(4, "result.Success(metadata.rowsAffected[0]);");
                builder.AppendTabStringLine(3, "} else {");
                builder.AppendTabStringLine(4, "result.Error('Update Fail');");
                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(1, "} catch (e) {");
                builder.AppendTabStringLine(2, "result.Error(e.message);");
                builder.AppendTabStringLine(1, "} finally {");
                builder.AppendTabStringLine(2, "return result;");
                builder.AppendTabStringLine(1, "}");
                builder.AppendLine("};");
            }

            return builder.ToString();
        }

        public string NodeSequelizeEntitiyCreate(BindOption options, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string entityName = info[0].TableName.ToLower();

                builder.AppendLine("const { DataTypes } = require('sequelize');");
                builder.AppendEmptyLine();
                builder.AppendLine($"module.exports = {entityName};");
                builder.AppendEmptyLine();
                builder.AppendLine($"function {entityName}(sequelize)");
                builder.AppendLine("{");
                builder.AppendTabStringLine(1, "const attributes = {");
                foreach(var item in info)
                {
                    if (item.is_identity)
                    {
                        builder.AppendTabString(2, $"{item.Name}: ");
                        builder.Append("{");
                        builder.Append($" type: DataTypes.{item.ColumnType.ToUpper()}");
                        if (item.IsSize)
                        {
                            builder.Append($"({item.max_length})");
                        }
                        builder.Append($", allowNull:false, primaryKey: true");
                        if (item.is_identity)
                        {
                            builder.Append(", autoIncrement: true");
                        }
                        builder.AppendLine("},");
                    }
                    else
                    {
                        builder.AppendTabString(2, $"{item.Name}: ");
                        builder.Append("{");
                        if (item.IsDate)
                        {
                            builder.Append($" type: DataTypes.DATE");
                        }
                        else
                        {
                            builder.Append($" type: DataTypes.{item.ColumnType.ToUpper()}");
                        }
                        if (item.IsSize)
                        {
                            builder.Append($"({item.max_length})");
                        }
                        builder.Append($", allowNull: {((item.is_nullable) ? "true" : "false")}");
                        if (!item.is_nullable)
                        {
                            switch (item.ScriptType)
                            {
                                case "number":
                                    builder.Append(", defaultValue: 0 ");
                                    break;
                                case "Date":
                                    builder.Append(", defaultValue: DataTypes.NOW ");
                                    break;
                                case "string":
                                    builder.Append(", defaultValue: '' ");
                                    break;
                                case "boolean":
                                    builder.Append(", defaultValue: false ");
                                    break;
                            }
                            
                        }
                        builder.AppendLine("},");
                    }
                    
                }
                builder.AppendTabStringLine(1, "};");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "const options = {");
                builder.AppendTabStringLine(2, $"tableName: \"{info[0].TableName}\",");
                if (info.Where(x => x.Name.Equals("Password", StringComparison.OrdinalIgnoreCase)).Count() > 0)
                {
                    builder.AppendTabStringLine(2, "defaultScope: {");
                    builder.AppendTabStringLine(3, "attributes: { exclude: ['Password'] }");
                    builder.AppendTabStringLine(2, "},");
                    builder.AppendTabStringLine(2, "scopes: {");
                    builder.AppendTabStringLine(3, "withHash: { attributes: {}, }");
                    builder.AppendTabStringLine(2, "},");
                }
                if (primaryKey != null)
                {
                    if (!(primaryKey.IsNumber && primaryKey.is_identity))
                    {
                        builder.AppendTabStringLine(2, "freezeTableName: true,");
                    }
                }
                builder.AppendTabStringLine(2, "timestamps: false");
                builder.AppendTabStringLine(1, "};");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, $"return sequelize.define('{entityName}', attributes, options);");
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        public string NodeSequelizeEntitiySaveMethod(BindOption options, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string entityName = info[0].TableName.ToLower();

                builder.Append($"exports.Save = async ({entityName.FirstCharToLower()}) => ");
                builder.AppendLine("{");
                builder.AppendTabStringLine(1, "let result = new ApiResult();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "try {");
                builder.AppendTabString(2, $"if ({entityName.FirstCharToLower()} !== null && {entityName.FirstCharToLower()} !== undefined)");
                builder.AppendLine(" {");
                builder.AppendTabString(3, $"let target = await db.{entityName.FirstCharToLower()}.findOne(");
                builder.AppendLine("{");
                builder.AppendTabString(4, "where : { ");
                builder.Append($"{primaryKey?.ColumnName} : {entityName.FirstCharToLower()}.{primaryKey?.ColumnName}");
                builder.AppendLine(" }");
                builder.AppendTabStringLine(3, "});");
                builder.AppendEmptyLine();
                builder.AppendTabString(3, $"if (target !== null && target !== undefined && target.{primaryKey?.ColumnName} > 0)");
                builder.AppendLine(" {");
                foreach (var item in info.Where(x => x.ColumnName != primaryKey?.ColumnName))
                {
                    builder.AppendTabStringLine(4, $"target.{item.ColumnName} = {entityName.FirstCharToLower()}.{item.ColumnName}");
                }
                builder.AppendTabStringLine(4, "await target.save();");
                builder.AppendEmptyLine();
                builder.AppendTabString(4, $"if (target.{primaryKey?.ColumnName} > 0) ");
                builder.AppendLine("{");
                builder.AppendTabStringLine(5, $"result.Success(target.{primaryKey?.ColumnName});");
                builder.AppendTabStringLine(4, "} else {");
                builder.AppendTabStringLine(5, "result.Error(\"수정에 실패하였습니다.\");");
                builder.AppendTabStringLine(4, "}");
                builder.AppendTabStringLine(3, "} else {");
                builder.AppendTabStringLine(4, $"let target = new db.{entityName.FirstCharToLower()}();");
                builder.AppendEmptyLine();
                foreach(var item in info.Where(x => x.ColumnName != primaryKey?.ColumnName))
                {
                    builder.AppendTabStringLine(4, $"target.{item.ColumnName} = {entityName.FirstCharToLower()}.{item.ColumnName}");
                }
                builder.AppendTabStringLine(4, "await target.save();");
                builder.AppendEmptyLine();
                builder.AppendTabString(4, $"if (target.{primaryKey?.ColumnName} > 0) ");
                builder.AppendLine("{");
                builder.AppendTabStringLine(5, $"result.Success(target.{primaryKey?.ColumnName});");
                builder.AppendTabStringLine(4, "} else {");
                builder.AppendTabStringLine(5, "result.Error(\"추가에 실패하였습니다.\");");
                builder.AppendTabStringLine(4, "}");
                builder.AppendTabStringLine(3, "}");
                
                
                

                builder.AppendTabStringLine(2, "} else {");
                builder.AppendTabStringLine(3, "result.Error(\"잘못된 접근입니다.\");");
                builder.AppendTabStringLine(2, "}");
                builder.AppendTabStringLine(1, "} catch (e) {");
                builder.AppendTabStringLine(2, "result.Error(e.message);");
                builder.AppendTabStringLine(1, "} finally {");
                builder.AppendTabStringLine(2, "return result;");
                builder.AppendTabStringLine(1, "}");
                builder.AppendLine("}");
            }

            return builder.ToString();
        }
    }
}
