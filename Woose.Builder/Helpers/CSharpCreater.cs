using System.Text;

namespace Woose.Builder
{
    public class CSharpCreater
    {
        public CSharpCreater()
        {
        }

        public string CreateEntity(OptionData options, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo paimaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (paimaryKey == null)
                {
                    paimaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                if (!options.IsInLine)
                {
                    builder.AppendLine($"public class {info[0].TableName} : IEntity");
                    builder.AppendLine("{");
                }
                foreach (var item in info)
                {
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
                builder.AppendTabStringLine(1, $"public {info[0].TableName}()");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(1, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "public string GetPaimaryColumn()");
                builder.AppendTabStringLine(1, "{");
                if (paimaryKey != null)
                {
                    builder.AppendTabStringLine(2, $"return \"{paimaryKey.Name}\";");
                }
                else
                {
                    builder.AppendTabStringLine(2, "return \"\";");
                }
                builder.AppendTabStringLine(1, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "public string GetTableName()");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(2, $"return \"{info[0].TableName}\";");
                builder.AppendTabStringLine(1, "}");
                if (!options.IsInLine)
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

        public string CreateApiMethod(OptionData options, List<SPEntity> info, List<SpTable> tables)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0 && tables != null && tables.Count > 0)
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

        public string CreateControllerMethod(OptionData options, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo paimaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (paimaryKey == null)
                {
                    paimaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string entityName = info[0].TableName;

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

                if (options.UsingCustomModel)
                {
                    builder.AppendTabStringLine(0, "//interface");
                    builder.AppendTabStringLine(0, $"public interface I{entityName}Repository : IRepository");
                    builder.AppendTabStringLine(0, "{");
                    builder.AppendTabStringLine(0, "}");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(0, "//repository");
                    builder.AppendTabStringLine(0, $"public class {entityName}Repository : BaseRepository, I{entityName}Repository");
                    builder.AppendTabStringLine(0, "{");
                    builder.AppendTabStringLine(1, $"public {entityName}Repository(IDatabaseService _context) : base(_context)");
                    builder.AppendTabStringLine(1, "{");
                    builder.AppendTabStringLine(1, "}");
                    builder.AppendTabStringLine(0, "}");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(0, "//Dependency Injection");
                    builder.AppendTabStringLine(0, $"service.AddSingleton<I{entityName}Repository, {entityName}Repository>();");
                    builder.AppendEmptyLine();
                }


                if (!options.IsInLine)
                {
                    builder.AppendLine($"[Route(\"api/[controller]\")]");
                    builder.AppendLine("[ApiController]");
                    builder.AppendLine($"public class {entityName}Controller : BaseController");
                    builder.AppendLine("{");
                    builder.AppendTabStringLine(1, $"protected I{entityName}Repository db;");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(1, $"public {entityName}Controller(IDatabaseService context, ICryptoHandler crypto, I{entityName}Repository _db) : base(context, crypto)");
                    builder.AppendTabStringLine(1, "{");
                    builder.AppendTabStringLine(2, "this.db = _db;");
                    builder.AppendTabStringLine(1, "}");
                    builder.AppendEmptyLine();
                }
                builder.AppendTabStringLine(1, "[Route(\"View/{idx}\")]");
                builder.AppendTabStringLine(1, "[HttpGet]");
                builder.AppendTabStringLine(1, $"public {getReturn} Get(long idx)");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(2, $"var result = new {getReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "var auth = this.GetAccessToken();");
                builder.AppendTabStringLine(2, "if (!string.IsNullOrWhiteSpace(auth.UserID))");
                builder.AppendTabStringLine(2, "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine(3, "var user = db.Single<Member>($\"Email='{auth.UserID}'\");");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberID > 0)");
                }
                else
                {
                    builder.AppendTabStringLine(3, "var user = db.GetMember(auth.UserID);");
                    builder.AppendTabStringLine(3, "//var user = db.GetManager(auth.UserID);");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberIDX > 0)");
                }

                builder.AppendTabStringLine(3, "{");

                builder.AppendTabStringLine(4, $"{entityName} {entityName.FirstCharToLower()} = db.Single<{entityName}>(idx);");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine(4, $"result.Success({entityName.FirstCharToLower()}.{paimaryKey.Name}, {entityName.FirstCharToLower()});");
                }
                else
                {
                    builder.AppendTabStringLine(4, $"result.Success({entityName.FirstCharToLower()});");
                }

                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(3, "else");
                builder.AppendTabStringLine(3, "{");
                builder.AppendTabStringLine(4, "result.Error(\"NotFound Member\");");
                builder.AppendTabStringLine(3, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "}");
                builder.AppendTabStringLine(2, "else");
                builder.AppendTabStringLine(2, "{");
                builder.AppendTabStringLine(3, "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine(2, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "return result;");
                builder.AppendTabStringLine(1, "}");

                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "[Route(\"List\")]");
                builder.AppendTabStringLine(1, "[HttpGet]");
                builder.AppendTabStringLine(1, $"public async Task<{listReturn}> List([FromQuery] PagingParameter paramData)");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(2, $"var result = new {listReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "var auth = this.GetAccessToken();");
                builder.AppendTabStringLine(2, "if (!string.IsNullOrWhiteSpace(auth.UserID))");
                builder.AppendTabStringLine(2, "{");

                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine(3, "var user = db.Single<Member>($\"Email='{auth.UserID}'\");");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberID > 0)");
                }
                else
                {
                    builder.AppendTabStringLine(3, "var user = db.GetMember(auth.UserID);");
                    builder.AppendTabStringLine(3, "//var user = db.GetManager(auth.UserID);");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberIDX > 0)");
                }
                builder.AppendTabStringLine(3, "{");

                builder.AppendTabStringLine(4, $"var option = paramData.toPagingOption<{entityName}>();");
                builder.AppendTabStringLine(4, $"var list = await Task.Factory.StartNew(() => db.List<{entityName}>(option)).ConfigureAwait(false);");
                builder.AppendTabStringLine(4, $"int cnt = await Task.Factory.StartNew(() => db.Count<{entityName}>(option)).ConfigureAwait(false);");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(4, $"if (list != null && list.Count() > 0)");
                builder.AppendTabStringLine(4, "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine(5, $"result.Success(cnt, list);");
                }
                else
                {
                    builder.AppendTabStringLine(5, $"result.Success(list);");
                    builder.AppendTabStringLine(5, "result.Count = cnt;");
                }

                builder.AppendTabStringLine(4, "}");
                builder.AppendTabStringLine(4, "else");
                builder.AppendTabStringLine(4, "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine(5, $"result.Success(0, new List<{entityName}>());");
                }
                else
                {
                    builder.AppendTabStringLine(5, $"result.Success(new List<{entityName}>());");
                    builder.AppendTabStringLine(5, "result.Count = 0;");
                }
                builder.AppendTabStringLine(4, "}");
                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(3, "else");
                builder.AppendTabStringLine(3, "{");
                builder.AppendTabStringLine(4, "result.Error(\"NotFound Member\");");
                builder.AppendTabStringLine(3, "}");

                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "}");
                builder.AppendTabStringLine(2, "else");
                builder.AppendTabStringLine(2, "{");
                builder.AppendTabStringLine(3, "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine(2, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "return result;");
                builder.AppendTabStringLine(1, "}");

                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "[Route(\"Save\")]");
                builder.AppendTabStringLine(1, "[HttpPost]");
                builder.AppendTabStringLine(1, $"public {exeReturn} Post([FromBody] {entityName} {entityName.FirstCharToLower()})");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(2, $"var result = new {exeReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "var auth = this.GetAccessToken();");
                builder.AppendTabStringLine(2, "if (!string.IsNullOrWhiteSpace(auth.UserID))");
                builder.AppendTabStringLine(2, "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine(3, "var user = db.Single<Member>($\"Email='{auth.UserID}'\");");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberID > 0)");
                }
                else
                {
                    builder.AppendTabStringLine(3, "var user = db.GetMember(auth.UserID);");
                    builder.AppendTabStringLine(3, "//var user = db.GetManager(auth.UserID);");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberIDX > 0)");
                }
                builder.AppendTabStringLine(3, "{");
                builder.AppendTabStringLine(4, "//실제 로직 구현");
                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(3, "else");
                builder.AppendTabStringLine(3, "{");
                builder.AppendTabStringLine(4, "result.Error(\"NotFound Member\");");
                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(2, "}");
                builder.AppendTabStringLine(2, "else");
                builder.AppendTabStringLine(2, "{");
                builder.AppendTabStringLine(3, "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine(2, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "return result;");
                builder.AppendTabStringLine(1, "}");

                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "[Route(\"Save/{id}\")]");
                builder.AppendTabStringLine(1, "[HttpPut]");
                builder.AppendTabStringLine(1, $"public {exeReturn} Put(long id, [FromBody] {entityName} {entityName.FirstCharToLower()})");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(2, $"var result = new {exeReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "var auth = this.GetAccessToken();");
                builder.AppendTabStringLine(2, "if (!string.IsNullOrWhiteSpace(auth.UserID))");
                builder.AppendTabStringLine(2, "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine(3, "var user = db.Single<Member>($\"Email='{auth.UserID}'\");");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberID > 0)");
                }
                else
                {
                    builder.AppendTabStringLine(3, "var user = db.GetMember(auth.UserID);");
                    builder.AppendTabStringLine(3, "//var user = db.GetManager(auth.UserID);");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberIDX > 0)");
                }
                builder.AppendTabStringLine(3, "{");
                builder.AppendTabStringLine(4, "//실제 로직 구현");
                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(3, "else");
                builder.AppendTabStringLine(3, "{");
                builder.AppendTabStringLine(4, "result.Error(\"NotFound Member\");");
                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(2, "}");
                builder.AppendTabStringLine(2, "else");
                builder.AppendTabStringLine(2, "{");
                builder.AppendTabStringLine(3, "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine(2, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "return result;");
                builder.AppendTabStringLine(1, "}");

                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "[Route(\"{id}\")]");
                builder.AppendTabStringLine(1, "[HttpDelete]");
                builder.AppendTabStringLine(1, $"public {exeReturn} Delete(long id)");
                builder.AppendTabStringLine(1, "{");
                builder.AppendTabStringLine(2, $"var result = new {exeReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "var auth = this.GetAccessToken();");
                builder.AppendTabStringLine(2, "if (!string.IsNullOrWhiteSpace(auth.UserID))");
                builder.AppendTabStringLine(2, "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine(3, "var user = db.Single<Member>($\"Email='{auth.UserID}'\");");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberID > 0)");
                }
                else
                {
                    builder.AppendTabStringLine(3, "var user = db.GetMember(auth.UserID);");
                    builder.AppendTabStringLine(3, "//var user = db.GetManager(auth.UserID);");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(3, $"if (user.MemberIDX > 0)");
                }
                builder.AppendTabStringLine(3, "{");
                builder.AppendTabStringLine(4, "//실제 로직 구현");
                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(3, "else");
                builder.AppendTabStringLine(3, "{");
                builder.AppendTabStringLine(4, "result.Error(\"NotFound Member\");");
                builder.AppendTabStringLine(3, "}");
                builder.AppendTabStringLine(2, "}");
                builder.AppendTabStringLine(2, "else");
                builder.AppendTabStringLine(2, "{");
                builder.AppendTabStringLine(3, "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine(2, "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "return result;");
                builder.AppendTabStringLine(1, "}");

                if (!options.IsInLine)
                {
                    builder.AppendLine("}");
                }

            }

            return builder.ToString();
        }
    }
}
