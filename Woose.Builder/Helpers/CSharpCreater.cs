using System.Text;
using Woose.Data;

namespace Woose.Builder
{
    public class CSharpCreater
    {
        public CSharpCreater()
        {
        }

        public string CreateEntity(BindOption options, List<DbTableInfo> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (properties != null && properties.Count > 0)
            {
                DbTableInfo paimaryKey = properties.Where(x => x.is_identity).FirstOrDefault();
                if (paimaryKey == null)
                {
                    paimaryKey = properties.OrderBy(x => x.column_id).FirstOrDefault();
                }

                if (IsNamespace)
                {
                    builder.AppendLine($"using System.Text.Json.Serialization;");
                    builder.AppendLine($"using Woose.Core;");
                    builder.AppendLine($"using Woose.Data;");
                    builder.AppendEmptyLine();
                    builder.AppendLine($"namespace {options.ProjectName}");
                    builder.AppendLine("{");
                }

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class {properties[0].TableName} : BaseEntity, IEntity");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");

                foreach (var item in properties)
                {
                    if (item.Name.Equals("IsEnabled", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"[JsonIgnore]");
                    }
                    builder.AppendTabString((IsNamespace ? 2 : 1), $"[Entity(\"{item.Name}\", System.Data.SqlDbType.{item.CsType}");
                    switch (item.ColumnType)
                    {
                        case "int":
                        case "money":
                            builder.Append(", 4");
                            break;
                        case "smallint":
                        case "smallmoney":
                            builder.Append(", 2");
                            break;
                        case "bit":
                        case "tinyint":
                            builder.Append(", 1");
                            break;
                        case "bigint":
                            builder.Append(", 8");
                            break;
                        case "datetime2":
                        case "datetime":
                        case "date":
                        case "time":
                            builder.Append(", 8");
                            break;
                        default:
                            if (item.IsSize && item.max_length == 0)
                            {
                                builder.Append($", -1");
                            }
                            else
                            {
                                builder.Append($", {item.max_length}");
                            }
                            break;
                    }

                    if (item.is_identity)
                    {
                        builder.Append(", true");
                    }
                    builder.AppendLine(")]");
                    builder.AppendTabString((IsNamespace ? 2 : 1), $"public {item.ObjectType} {item.Name}");
                    builder.Append(" { get; set; }");
                    switch (item.ObjectType)
                    {
                        case "int":
                        case "double":
                            builder.AppendLine(" = 0;");
                            break;
                        case "long":
                            builder.AppendLine(" = -1;");
                            break;
                        case "DateTime":
                            builder.AppendLine(" = new DateTime();");
                            break;
                        case "bool":
                            builder.AppendLine(" = false;");
                            break;
                        case "string":
                            builder.AppendLine(" = string.Empty;");
                            break;
                    }
                    builder.AppendEmptyLine();
                }
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {properties[0].TableName}()");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"this.TableName = \"{properties[0].TableName}\";");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"this.PrimaryColumn = \"{paimaryKey.Name}\";");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "}");

                if (IsNamespace)
                {
                    builder.AppendLine("}");
                }
            }

            return builder.ToString();
        }

        public string CreateSP(OptionData options, List<SPEntity> properties, List<SpTable> tables, List<SpOutput> outputs)
        {
            StringBuilder builder = new StringBuilder(200);

            //options.ReturnType
            //Void
            //ExecuteResult
            //Entity T Bind
            //Entities List Bind

            string spName = string.Empty;
            string mainTable = string.Empty;
            string returnModel = string.Empty;
            string funcName = string.Empty;
            DbTypeItem typeObj = null;

            if (tables != null && tables.Count > 0)
            {
                spName = tables[0].name;
                mainTable = tables[0].TableName;
                funcName = spName.Replace("USP_", "").Replace("_", "").Trim();
            }
            else
            {
                spName = "USP_Custom_StoredProcedure";
                funcName = "CustomSp";
            }

            if (options.UsingCustomModel && outputs != null && outputs.Count > 0)
            {
                builder.AppendLine($"//반환 파라미터 모델");
                builder.AppendLine($"public class {funcName}Item");
                builder.AppendLine("{");
                
                foreach (var item in outputs)
                {
                    typeObj = DbTypeHelper.MSSQL.ParseColumnType(item.system_type_name);
                    builder.AppendTabString(1, $"public {DbTypeHelper.MSSQL.GetObjectTypeByCsharp(typeObj.Name)} {item.name}");
                    builder.Append(" { get; set; }");
                    builder.AppendLine($" = {DbTypeHelper.MSSQL.GetObjectDefaultValueByCsharp(typeObj.Name)};");
                }
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, $"public {funcName}Item()");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(1, "}");
                builder.AppendLine("}");
                builder.AppendEmptyLine();

                switch (options.ReturnType)
                {
                    case "Void":
                        returnModel = "void";
                        break;
                    case "BindModel":
                        returnModel = options.BindModel;
                        break;
                    case "Entity T Bind":
                        returnModel = $"{funcName}Item";
                        break;
                    case "Entities List Bind":
                        returnModel = $"List<{funcName}Item>";
                        break;
                }
            }
            else
            {
                switch (options.ReturnType)
                {
                    case "Void":
                        returnModel = "void";
                        break;
                    case "BindModel":
                        returnModel = options.BindModel;
                        break;
                    case "Entity T Bind":
                        returnModel = (string.IsNullOrWhiteSpace(options.ReturnModel)) ? "dynamic" : options.ReturnModel;
                        break;
                    case "Entities List Bind":
                        returnModel = (string.IsNullOrWhiteSpace(options.ReturnModel)) ? "List<dynamic>" : $"List<{options.ReturnModel}>";
                        break;
                }
            }

            if (options.UsingCustomModel && properties != null && properties.Count > 0)
            {
                mainTable = $"Input{funcName}";

                builder.AppendLine($"//입력 파라미터 모델");
                builder.AppendLine($"public class Input{funcName}");
                builder.AppendLine("{");
                foreach (var item in properties.Where(x => !x.is_output))
                {
                    builder.AppendTabString(1, $"public {DbTypeHelper.MSSQL.GetObjectTypeByCsharp(item.type)} {item.name.Replace("@","")}");
                    builder.Append(" { get; set; }");
                    builder.AppendLine($" = {DbTypeHelper.MSSQL.GetObjectDefaultValueByCsharp(item.type)};");
                }
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, $"public Input{funcName}()");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(1, "}");
                builder.AppendLine("}");
                builder.AppendEmptyLine();
            }

            if (!options.IsInLine)
            {
                if (options.IsNoModel)
                {
                    builder.Append($"public {returnModel} {funcName}(");
                    if (properties != null && properties.Count > 0)
                    {
                        int c = 0;
                        foreach (var item in properties.Where(x => x.is_output == false))
                        {
                            if (c > 0)
                            {
                                builder.Append(",");
                            }
                            builder.Append($"{DbTypeHelper.MSSQL.GetObjectTypeByCsharp(item.type)} {item.name.Replace("@", "")}");
                            c++;
                        }
                    }
                    builder.AppendLine(")");
                }
                else
                {
                    builder.AppendLine($"public {returnModel} {funcName}(Input{funcName} {mainTable.ToLower()})");
                }
                builder.AppendLine("{");
            }
            builder.AppendTabStringLine(1, $"var result = new {returnModel}();");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine(1, "using (var db = new DbHelper(context.GetConnection()))");
            builder.AppendTabStringLine(1, $"using (var cmd = db.CreateSP(\"{spName}\"))");
            builder.AppendTabStringLine(1, "{");
            if (properties != null && properties.Count > 0)
            {
                foreach (var item in properties)
                {
                    if (!item.is_output)
                    {
                        if (options.IsNoModel)
                        {
                            builder.AppendTabStringLine(2, $"cmd.Parameters.Set(\"{item.name}\", SqlDbType.{item.DbTypeString}, {item.name.Replace("@", "")}, {((item.DbTypeString.Substring(0, 1) == "N") ? item.max_length / 2 : item.max_length)});");
                        }
                        else
                        {
                            builder.AppendTabStringLine(2, $"cmd.Parameters.Set(\"{item.name}\", SqlDbType.{item.DbTypeString}, {mainTable.ToLower()}.{item.name.Replace("@", "")}, {((item.DbTypeString.Substring(0, 1) == "N") ? item.max_length / 2 : item.max_length)});");
                        }
                    }
                }
            }
            builder.AppendEmptyLine();
            switch (options.ReturnType)
            {
                case "Void":
                    builder.AppendTabStringLine(2, "var cnt = cmd.NoneExecuteResult();");
                    builder.AppendTabStringLine(2, "if (cnt != null && cnt > 0)");
                    builder.AppendTabStringLine(2, "{");
                    builder.AppendTabStringLine(3, "result.Success(cnt)");
                    builder.AppendTabStringLine(2, "}");
                    break;
                case "BindModel":
                    if (options.BindModel == OptionData.BindModelType.ExecuteResult.ToString())
                    {
                        builder.AppendTabStringLine(2, "var dt = cmd.ExecuteTable();");
                        builder.AppendTabStringLine(2, "if (dt != null && dt.Rows.Count > 0)");
                        builder.AppendTabStringLine(2, "{");
                        builder.AppendTabStringLine(3, "result = EntityBinder.ColumnToEntity<ExecuteResult>(dt);");
                        builder.AppendTabStringLine(2, "}");
                    }
                    else
                    {
                        builder.AppendTabStringLine(2, "result = cmd.ExecuteReturnValues();");
                    }
                    break;
                case "Entity T Bind":
                    builder.AppendTabStringLine(2, $"var dt = cmd.ExecuteTable();");
                    builder.AppendTabStringLine(2, "if (dt != null && dt.Rows.Count > 0)");
                    builder.AppendTabStringLine(2, "{");
                    builder.AppendTabStringLine(3, $"result = EntityBinder.ColumnToEntity<{returnModel}>(dt);");
                    builder.AppendTabStringLine(2, "}");
                    break;
                case "Entities List Bind":
                    builder.AppendTabStringLine(2, $"var dt = cmd.ExecuteTable();");
                    builder.AppendTabStringLine(2, "if (dt != null && dt.Rows.Count > 0)");
                    builder.AppendTabStringLine(2, "{");
                    builder.AppendTabStringLine(3, $"result = EntityBinder.ColumnToEntities<{returnModel.Replace("List<","").Replace(">", "")}>(dt);");
                    builder.AppendTabStringLine(2, "}");
                    break;
            }
            if (options.BindModel == OptionData.BindModelType.ExecuteResult.ToString())
            {
                builder.AppendTabStringLine(2, "else");
                builder.AppendTabStringLine(2, "{");
                builder.AppendTabStringLine(3, "result.Error(\"반환값이 없습니다.\");");
                builder.AppendTabStringLine(2, "}");
            }
            builder.AppendTabStringLine(1, "}");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine(1, "return result;");
            if (!options.IsInLine)
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        public string CreateApiMethod(BindOption options, List<SPEntity> properties, List<SpTable> tables, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (properties != null && properties.Count > 0 && tables != null && tables.Count > 0)
            {
                string spName = tables[0].name;
                string mainTable = tables[0].TableName;
                string method = spName.Replace("USP_", "").Replace("_", "").Trim();

                string returnModel = string.Empty;

                switch (options.ReturnType)
                {
                    case "Void":
                        returnModel = "void";
                        break;
                    case "BindModel":
                        if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                        {
                            returnModel = $"ReturnValue";
                        }
                        else
                        {
                            returnModel = "ApiResult<ExecuteResult>";
                        }
                        break;
                    case "Entity T Bind":
                        if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                        {

                            returnModel = $"ReturnValues<{mainTable}>";
                        }
                        else
                        {
                            returnModel = $"ApiResult<{mainTable}>";
                        }
                        break;
                    case "Entities List Bind":
                        if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                        {
                            returnModel = $"ReturnValues<List<{mainTable}>>";
                        }
                        else
                        {
                            returnModel = $"ApiResult<List<{mainTable}>>";
                        }
                        break;
                }

                builder.AppendLine($"[HttpPost]");
                if (options.IsNoModel)
                {
                    builder.AppendLine($"public {returnModel} {method}([FromBody] {mainTable} {mainTable.ToLower()})");
                }
                else
                {
                    builder.AppendLine($"public {returnModel} {method}([FromBody] {mainTable} {mainTable.ToLower()})");
                }
                builder.AppendLine("{");
                builder.AppendTabStringLine(1, $"var result = new {returnModel}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "var auth = this.GetAccessToken();");
                builder.AppendTabStringLine(1, "if (!string.IsNullOrWhiteSpace(auth.UserID))");
                builder.AppendTabStringLine(1, "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                { 
                    builder.AppendTabStringLine(2, "var user = db.Single<Member>($\"Email='{auth.UserID}'\");");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(2, "if (user.MemberID > 0)");
                    builder.AppendTabStringLine(2, "{");
                    builder.AppendTabStringLine(3, $"result = db.{method}({mainTable.ToLower()});");
                }
                else
                {
                    builder.AppendTabStringLine(2, "var user = db.GetMember(auth.UserID);");
                    builder.AppendTabStringLine(2, "//var user = db.AuthCheck(auth.UserID);");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(2, "if (user.MemberIDX > 0)");
                    builder.AppendTabStringLine(2, "{");
                    builder.AppendTabStringLine(3, $"var tmp = db.{method}({mainTable.ToLower()});");
                    builder.AppendTabStringLine(3, "if (tmp != null)");
                    builder.AppendTabStringLine(3, "{");
                    builder.AppendTabStringLine(3, "result = tmp.ToResult();");
                    builder.AppendTabStringLine(3, "}");
                    builder.AppendTabStringLine(3, "else");
                    builder.AppendTabStringLine(3, "{");
                    builder.AppendTabStringLine(3, "result.Error(\"Fail Data Save\");");
                    builder.AppendTabStringLine(3, "}");
                }
                builder.AppendTabStringLine(2, "}");
                builder.AppendTabStringLine(2, "else");
                builder.AppendTabStringLine(2, "{");
                builder.AppendTabStringLine(3, "result.Error(\"NotFound Account\");");
                builder.AppendTabStringLine(2, "}");
                builder.AppendTabStringLine(1, "}");
                builder.AppendTabStringLine(1, "else");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(2, "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine(1, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "return result;");
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        public string CreateController(BindOption options, List<DbTableInfo> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (properties != null && properties.Count > 0)
            {
                DbTableInfo paimaryKey = properties.Where(x => x.is_identity).FirstOrDefault();
                if (paimaryKey == null)
                {
                    paimaryKey = properties.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string entityName = properties[0].TableName;

                string getReturn = string.Empty;
                string listReturn = string.Empty;
                string exeReturn = string.Empty;
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    getReturn = $"ReturnValues<{entityName}>";
                    listReturn = $"ReturnValues<List<{entityName}>>";
                    exeReturn = "ReturnValue";
                }
                else
                {
                    getReturn = $"ApiResult<{entityName}>";
                    listReturn = $"ApiResult<List<{entityName}>>";
                    exeReturn = "ApiResult<ExecuteResult>";
                }


                if (IsNamespace)
                {
                    builder.AppendLine($"using Microsoft.AspNetCore.Mvc;");
                    builder.AppendLine($"using Woose.Core;");
                    builder.AppendLine($"using Woose.Data;");
                    builder.AppendLine($"using Woose.API;");
                    builder.AppendEmptyLine();
                    builder.AppendLine($"namespace {options.ProjectName}");
                    builder.AppendLine("{");
                }

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"[Route(\"api/[controller]\")]");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "[ApiController]");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class {entityName}Controller : BaseController");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
                builder.AppendTabStringLine((IsNamespace ? 2 :1), $"protected I{options.MethodName}Repository db;");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {entityName}Controller(IContext context, ICryptoHandler crypto, IConfiguration config, I{options.MethodName}Repository _db) : base(context,crypto,config)");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "this.db = _db;");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[Route(\"View/{idx}\")]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[HttpGet]");
                if (options.IsAsync)
                {
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public async Task<{getReturn}> Get(long idx)");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {getReturn} Get(long idx)");
                }
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {getReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "var user = this.GetAccessToken();");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (user != null && !string.IsNullOrWhiteSpace(user.Id))");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "using (var db = context.getConnection())");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "using (var handler = new SqlDbOperater(db))");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "{");
                if (options.IsAsync)
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"{entityName} {entityName.FirstCharToLower()} = await Entity<{entityName}>.Run.On(handler)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".Select(1)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".Where(x => x.{paimaryKey.Name} == idx)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".ToEntityAsync();");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"{entityName} {entityName.FirstCharToLower()} = Entity<{entityName}>.Run.On(handler)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".Select(1)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".Where(x => x.{paimaryKey.Name} == idx)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".ToEntity();");
                }
                builder.AppendEmptyLine();
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result.Success({entityName.FirstCharToLower()}.{paimaryKey.Name}, {entityName.FirstCharToLower()});");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result.Success({entityName.FirstCharToLower()});");
                }
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "}");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "else");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");

                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[Route(\"List\")]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[HttpGet]");
                if (options.IsAsync)
                {
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public async Task<{listReturn}> List([FromQuery] IPagingParameter paramData)");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {listReturn} List([FromQuery] IPagingParameter paramData)");
                }
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {listReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "var user = this.GetAccessToken();");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (user != null && !string.IsNullOrWhiteSpace(user.Id))");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");

                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "using (var db = context.getConnection())");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "using (var handler = new SqlDbOperater(db))");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "{");
                if (options.IsAsync)
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"var list = await Entity<{entityName}>.Run.On(handler)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".Paging(10, paramData.CurPage)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".OrderBy(x => x.{paimaryKey.Name}, QueryOption.Sequence.DESC)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".ToListAsync();");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"int cnt = await Entity<{entityName}>.Run.On(handler)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".Count()");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".OrderBy(x => x.{paimaryKey.Name}, QueryOption.Sequence.DESC)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".ToCountAsync();");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"var list = Entity<{entityName}>.Run.On(handler)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".Paging(10, paramData.CurPage)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".OrderBy(x => x.{paimaryKey.Name}, QueryOption.Sequence.DESC)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".ToList();");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"int cnt = Entity<{entityName}>.Run.On(handler)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".Count()");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".OrderBy(x => x.{paimaryKey.Name}, QueryOption.Sequence.DESC)");
                    builder.AppendTabStringLine((IsNamespace ? 7 : 6), $".ToCount();");
                }

                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"if (list != null && list.Count() > 0)");
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 6 : 5), $"result.Success(cnt, list);");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 6 : 5), $"result.Success(list);");
                    builder.AppendTabStringLine((IsNamespace ? 6 : 5), "result.Count = cnt;");
                }

                builder.AppendTabStringLine((IsNamespace ? 5 : 4), "}");
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), "else");
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 6 : 5), $"result.Success(0, new List<{entityName}>());");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 6 : 5), $"result.Success(new List<{entityName}>());");
                    builder.AppendTabStringLine((IsNamespace ? 6 : 5), "result.Count = 0;");
                }
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), "}");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "else");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");

                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[Route(\"Save\")]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[HttpPost]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {exeReturn} Post([FromBody] {entityName} {entityName.FirstCharToLower()})");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {exeReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "var user = this.GetAccessToken();");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (user != null && !string.IsNullOrWhiteSpace(user.Id))");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), "//실제 로직 구현");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "else");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");

                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[Route(\"{idx}\")]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[HttpDelete]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {exeReturn} Delete(long idx)");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {exeReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "var user = this.GetAccessToken();");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (user != null && !string.IsNullOrWhiteSpace(user.Id))");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), "//실제 로직 구현");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "else");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
                builder.AppendLine("}");
                if (IsNamespace)
                {
                    builder.AppendLine("}");
                }
            }

            return builder.ToString();
        }

        public string CreateAbstract(BindOption options, DbContext context, List<DbEntity> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (properties != null && properties.Count > 0)
            {
                if (IsNamespace)
                {
                    builder.AppendLine($"using Woose.API;");
                    builder.AppendEmptyLine();
                    builder.AppendLine($"namespace {options.ProjectName}");
                    builder.AppendLine("{");
                }

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public interface I{options.MethodName}Repository : IRepository");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "}");
                
                if (IsNamespace)
                {
                    builder.AppendLine("}");
                }
            }

            return builder.ToString();
        }

        public string CreateRepository(BindOption options, DbContext context, List<DbEntity> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (IsNamespace)
            {
                builder.AppendLine($"using Woose.Core;");
                builder.AppendLine($"using Woose.Data;");
                builder.AppendLine($"using Woose.API;");
                builder.AppendEmptyLine();
                builder.AppendLine($"namespace {options.ProjectName}");
                builder.AppendLine("{");
            }

            builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class {options.MethodName}Repository : BaseRepository, I{options.MethodName}Repository");
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {options.MethodName}Repository(IContext context) : base(context)");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            if (properties != null && properties.Count > 0)
            {
                foreach (var sp in properties)
                {
                    builder.AppendLine(CreateSPItem(options, context, sp, IsNamespace));
                }
            }
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "}");

            if (IsNamespace)
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        private string CreateSPItem(BindOption options, DbContext context, DbEntity property, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            string exeReturn = string.Empty;

            if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
            {
                exeReturn = "ReturnValue";
            }
            else
            {
                exeReturn = "ExecuteResult";
            }

            builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"public {exeReturn} {GetNameFromSP(property.name)}()");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");

            return builder.ToString();
        }

        public static string CreateEntity(BindOption options, List<DbTableInfo> properties)
        {
            StringBuilder builder = new StringBuilder(200);

            if (properties != null && properties.Count > 0)
            {
                DbTableInfo paimaryKey = properties.Where(x => x.is_identity).FirstOrDefault();
                if (paimaryKey == null)
                {
                    paimaryKey = properties.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string tableName = properties[0].TableName;

                builder.AppendLine($"public class {tableName} : BaseEntity, IEntity");
                builder.AppendLine("{");

                foreach (var item in properties)
                {
                    if (item.Name.Equals("IsEnabled", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.AppendTabStringLine(1, $"[JsonIgnore]");
                    }
                    builder.AppendTabString(1, $"[Entity(\"{item.Name}\", System.Data.SqlDbType.{item.CsType}");
                    switch (item.ColumnType)
                    {
                        case "int":
                        case "money":
                            builder.Append(", 4");
                            break;
                        case "smallint":
                        case "smallmoney":
                            builder.Append(", 2");
                            break;
                        case "bit":
                        case "tinyint":
                            builder.Append(", 1");
                            break;
                        case "bigint":
                            builder.Append(", 8");
                            break;
                        case "datetime2":
                        case "datetime":
                        case "date":
                        case "time":
                            builder.Append(", 8");
                            break;
                        default:
                            if (item.IsSize && item.max_length == 0)
                            {
                                builder.Append($", -1");
                            }
                            else
                            {
                                builder.Append($", {item.max_length}");
                            }
                            break;
                    }

                    if (item.is_identity)
                    {
                        builder.Append(", true");
                    }
                    builder.AppendLine(")]");
                    builder.AppendTabString(1, $"public {item.ObjectType} {item.Name}");
                    builder.Append(" { get; set; }");
                    switch (item.ObjectType)
                    {
                        case "int":
                        case "double":
                            builder.AppendLine(" = 0;");
                            break;
                        case "long":
                            builder.AppendLine(" = -1;");
                            break;
                        case "DateTime":
                            builder.AppendLine(" = new DateTime();");
                            break;
                        case "bool":
                            builder.AppendLine(" = false;");
                            break;
                        case "string":
                            builder.AppendLine(" = string.Empty;");
                            break;
                    }
                    builder.AppendEmptyLine();
                }
                builder.AppendTabStringLine(1, $"public {tableName}()");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(2, $"this.TableName = \"{tableName}\";");
                builder.AppendTabStringLine(2, $"this.PrimaryColumn = \"{paimaryKey.Name}\";");
                builder.AppendTabStringLine(1, "}");
                builder.AppendEmptyLine();
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        private string GetNameFromSP(string spname)
        {
            string result = string.Empty;

            if (!string.IsNullOrWhiteSpace(spname))
            {
                if (spname.IndexOf("_") > -1)
                {
                    string[] arr = spname.Split('_');
                    if (arr.Length > 1)
                    {
                        result = "";
                        for(int i = 1; i < arr.Length; i++)
                        {
                            result += arr[i];
                        }
                    }
                    else
                    {
                        result = spname.Replace("_", "");
                    }
                }
            }

            return result;
        }
    }
}
