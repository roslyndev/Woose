using System.Text;
using System.Windows.Documents;
using System.Xml.Linq;
using Woose.Data;
using Woose.Core;

namespace Woose.Builder
{
    public class CSharpCreater
    {
        public CSharpCreater()
        {
        }

        public string CreateEntity(BindOption options, DbEntity entity, List<DbTableInfo> properties, bool IsNamespace = false)
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

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class {entity.name} : BaseEntity, IEntity");
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
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {entity.name}()");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"this.TableName = \"{entity.name}\";");
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

        public string CreateSP(BindOption options, List<SPEntity> properties, List<SpTable> tables, List<SpOutput> outputs)
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
            string singleModel = string.Empty;

            if (tables != null && tables.Count > 0)
            {
                spName = tables[0].name;
                mainTable = tables[0].TableName;
                funcName = GetNameFromSP(spName);
            }
            else
            {
                spName = "USP_Custom_StoredProcedure";
                funcName = "CustomSp";
            }

            if (outputs != null && outputs.Count > 0)
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
                        returnModel = $"{funcName}Item";
                        break;
                    case "Entities List Bind":
                        returnModel = $"List<{funcName}Item>";
                        singleModel = $"{funcName}Item";
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
                        returnModel = (string.IsNullOrWhiteSpace(funcName)) ? "dynamic" : funcName;
                        break;
                    case "Entities List Bind":
                        returnModel = (string.IsNullOrWhiteSpace(funcName)) ? "List<dynamic>" : $"List<{funcName}>";
                        break;
                }
            }

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
            if (!returnModel.Equals("void", StringComparison.OrdinalIgnoreCase))
            {
                builder.AppendTabStringLine(1, $"var result = new {returnModel}();");
            }
            builder.AppendEmptyLine();
            builder.AppendTabStringLine(1, "using (var db = context.getConnection())");
            builder.AppendTabStringLine(1, $"using (var cmd = db.CreateCommand())");
            builder.AppendTabStringLine(1, "{");
            builder.AppendTabStringLine(2, $"cmd.On(\"{spName}\").Set();");
            foreach (var input in properties)
            {
                builder.AppendTabString(2, $"cmd.Parameters.Set(\"{input.name}\", SqlDbType.{input.CsType}, ");
                if (options.IsNoModel)
                {
                    builder.Append(input.name.FirstCharToLower());
                }
                else
                {
                    builder.Append($"input{GetNameFromSP(input.name)}.{input.name.Replace("@", "")}");
                }
                if (input.IsSize)
                {
                    if (input.CsType.Equals("NVarChar", StringComparison.OrdinalIgnoreCase) || input.CsType.Equals("VarChar", StringComparison.OrdinalIgnoreCase))
                    {
                        if (input.max_length < 1)
                        {
                            builder.Append($", -1");
                        }
                        else
                        {
                            builder.Append($", {input.max_length}");
                        }
                    }
                    else
                    {
                        builder.Append($", {input.max_length}");
                    }

                }
                builder.AppendLine(");");
            }

            switch (options.ReturnType)
            {
                case "Void":
                    builder.AppendTabStringLine(2, $"cmd.ExecuteNonQuery();");
                    break;
                case "BindModel":
                    if (options.BindModel == OptionData.BindModelType.ExecuteResult.ToString())
                    {
                        builder.AppendTabStringLine(2, $"result = cmd.ExecuteResult();");
                    }
                    else
                    {
                        builder.AppendTabStringLine(2, $"result = cmd.ExecuteReturnValue();");
                    }
                    break;
                case "Entity T Bind":
                    builder.AppendTabStringLine(2, $"result = cmd.ExecuteEntity<{returnModel}>();");
                    break;
                case "Entities List Bind":
                    builder.AppendTabStringLine(2, $"result = cmd.ExecuteEntities<{singleModel}>();");
                    break;
            }
            builder.AppendTabStringLine(1, "}");
            
            if (!returnModel.Equals("void", StringComparison.OrdinalIgnoreCase))
            {
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(1, "return result;");
            }
            builder.AppendLine("}");

            return builder.ToString();
        }

        public string CreateSPInterface(BindOption options, List<SPEntity> properties, List<SpTable> tables, List<SpOutput> outputs)
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
                funcName = GetNameFromSP(spName);
            }
            else
            {
                spName = "USP_Custom_StoredProcedure";
                funcName = "CustomSp";
            }

            if (outputs != null && outputs.Count > 0)
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
                        returnModel = (string.IsNullOrWhiteSpace(funcName)) ? "dynamic" : funcName;
                        break;
                    case "Entities List Bind":
                        returnModel = (string.IsNullOrWhiteSpace(funcName)) ? "List<dynamic>" : $"List<{funcName}>";
                        break;
                }
            }

            if (options.IsNoModel)
            {
                builder.Append($"{returnModel} {funcName}(");
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
                builder.AppendLine(");");
            }
            else
            {
                builder.AppendLine($"{returnModel} {funcName}(Input{funcName} {mainTable.ToLower()});");
            }

            return builder.ToString();
        }

        public string CreateSPEntity(BindOption options, List<SPEntity> properties, List<SpTable> tables, List<SpOutput> outputs)
        {
            StringBuilder builder = new StringBuilder(200);

            //options.ReturnType
            //Void
            //ExecuteResult
            //Entity T Bind
            //Entities List Bind

            string spName = string.Empty;
            string funcName = string.Empty;
            DbTypeItem typeObj = null;

            if (tables != null && tables.Count > 0)
            {
                spName = tables[0].name;
                funcName = GetNameFromSP(spName);
            }
            else
            {
                spName = "USP_Custom_StoredProcedure";
                funcName = "CustomSp";
            }

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

            builder.AppendLine($"//입력 파라미터 모델");
            builder.AppendLine($"public class Input{funcName}");
            builder.AppendLine("{");
            foreach (var item in properties.Where(x => !x.is_output))
            {
                builder.AppendTabString(1, $"public {DbTypeHelper.MSSQL.GetObjectTypeByCsharp(item.type)} {item.name.Replace("@", "")}");
                builder.Append(" { get; set; }");
                builder.AppendLine($" = {DbTypeHelper.MSSQL.GetObjectDefaultValueByCsharp(item.type)};");
            }
            builder.AppendEmptyLine();
            builder.AppendTabStringLine(1, $"public Input{funcName}()");
            builder.AppendTabStringLine(1, "{");
            builder.AppendTabStringLine(1, "}");
            builder.AppendLine("}");


            return builder.ToString();
        }

        public string CreateApiMethod(BindOption options, List<SPEntity> properties, List<SpTable> tables, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);
            int num = 0;

            if (properties != null && properties.Count > 0 && tables != null && tables.Count > 0)
            {
                string spName = tables[0].name;
                string mainTable = $"{GetNameFromSP(spName)}Item";
                string method = GetNameFromSP(spName);

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

                builder.AppendLine((string.IsNullOrWhiteSpace(options.MethodType)) ? "[HttpPost]" : $"[{options.MethodType}]");
                if (options.IsNoModel)
                {
                    builder.Append($"public {returnModel} {method}(");
                    num = 0;
                    foreach(var property in properties)
                    {
                        if (num > 0) builder.Append(",");
                        builder.Append($"[{GetFrom(options.MethodType)}] {property.ObjectType} {property.Name}");
                        num++;
                    }
                    builder.AppendLine(")");
                }
                else
                {
                    builder.AppendLine($"public {returnModel} {method}([{GetFrom(options.MethodType)}] {mainTable} {mainTable.ToLower()})");
                }
                builder.AppendLine("{");
                if (!returnModel.Equals("void", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AppendTabStringLine(1, $"var result = new {returnModel}();");
                    builder.AppendEmptyLine();
                }
                builder.AppendTabStringLine(1, "var user = this.GetAccessToken();");
                builder.AppendTabStringLine(1, "if (user != null && !string.IsNullOrWhiteSpace(user.ServerToken) && user.ServerToken == AppSettings.Current.ServerToken)");
                builder.AppendTabStringLine(1, "{");

                switch (options.ReturnType)
                {
                    case "Void":
                        builder.AppendTabStringLine(2, $"db.{method}({mainTable.ToLower()});");
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
                            builder.AppendTabStringLine(2, $"var tmp = db.{method}({mainTable.ToLower()});");
                            builder.AppendTabStringLine(2, "if (tmp != null)");
                            builder.AppendTabStringLine(2, "{");
                            builder.AppendTabStringLine(3, "result.Success(1, tmp);");
                            builder.AppendTabStringLine(2, "}");
                        }
                        else
                        {
                            builder.AppendTabStringLine(2, $"var tmp = db.{method}({mainTable.ToLower()});");
                            builder.AppendTabStringLine(2, "if (tmp != null)");
                            builder.AppendTabStringLine(2, "{");
                            builder.AppendTabStringLine(3, "result = tmp.ToResult();");
                            builder.AppendTabStringLine(2, "}");
                        }
                        break;
                    case "Entities List Bind":
                        if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                        {
                            builder.AppendTabStringLine(2, $"var tmp = db.{method}({mainTable.ToLower()});");
                            builder.AppendTabStringLine(2, "if (tmp != null)");
                            builder.AppendTabStringLine(2, "{");
                            builder.AppendTabStringLine(3, "result.Success(tmp.Count, tmp);");
                            builder.AppendTabStringLine(2, "}");
                        }
                        else
                        {
                            builder.AppendTabStringLine(2, $"var tmp = db.{method}({mainTable.ToLower()});");
                            builder.AppendTabStringLine(2, "if (tmp != null)");
                            builder.AppendTabStringLine(2, "{");
                            builder.AppendTabStringLine(3, "result.Success(tmp);");
                            builder.AppendTabStringLine(3, "result.Count = tmp.Count;");
                            builder.AppendTabStringLine(2, "}");
                        }
                        break;
                }

                if (!returnModel.Equals("void", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AppendTabStringLine(2, "else");
                    builder.AppendTabStringLine(2, "{");
                    builder.AppendTabStringLine(3, "result.Error(\"Fail Data Save\");");
                    builder.AppendTabStringLine(2, "}");
                }

                builder.AppendTabStringLine(1, "}");
                if (!returnModel.Equals("void", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AppendTabStringLine(1, "else");
                    builder.AppendTabStringLine(1, "{");
                    builder.AppendTabStringLine(2, "result.Error(\"Authorization header not found\");");
                    builder.AppendTabStringLine(1, "}");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(1, "return result;");
                }
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        public string CreateController(BindOption options, DbEntity entity, List<DbTableInfo> properties, DbContext context, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            var sps = new List<DbEntity>();

            using(var rep = new SqlServerRepository(context))
            {
                sps = rep.GetSpEntities();
            }

            if (properties != null && properties.Count > 0)
            {
                DbTableInfo paimaryKey = properties.Where(x => x.is_identity).FirstOrDefault();
                if (paimaryKey == null)
                {
                    paimaryKey = properties.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string entityName = entity.name;

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
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class {entityName}Controller : DefaultController");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
                builder.AppendTabStringLine((IsNamespace ? 2 :1), $"protected I{entityName}Repository db;");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {entityName}Controller(IContext context, ICryptoHandler crypto, IConfiguration config, I{entityName}Repository _db) : base(context,crypto,config)");
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
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (user != null && !string.IsNullOrWhiteSpace(user.ServerToken) && user.ServerToken == AppSettings.Current.ServerToken)");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "var tmp = db.Single(idx);");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result.Success(tmp.{paimaryKey.Name}, tmp);");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result.Success(tmp);");
                }
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
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public async Task<{listReturn}> List([FromQuery] PagingParameter paramData)");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {listReturn} List([FromQuery] PagingParameter paramData)");
                }
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {listReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "var user = this.GetAccessToken();");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (user != null && !string.IsNullOrWhiteSpace(user.ServerToken) && user.ServerToken == AppSettings.Current.ServerToken)");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");

                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "var list = db.List(paramData);");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "int cnt = db.Count(paramData);");

                builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"if (list != null && list.Count() > 0)");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result.Success(cnt, list);");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result.Success(list);");
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), "result.Count = cnt;");
                }

                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "}");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "else");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "{");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result.Success(0, new List<{entityName}>());");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result.Success(new List<{entityName}>());");
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), "result.Count = 0;");
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

                if (sps != null && sps.Count > 0)
                {
                    foreach(var sp in sps)
                    {
                        if (CodeHelper.ContainEntitySaveSP(sp.name, out string inEntityName))
                        {
                            if (inEntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase))
                            {
                                builder.AppendEmptyLine();
                                builder.AppendLine(CreateControllerSP(options, context, sp, false, 1, "Save"));
                            }
                        }
                    }
                }

                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[Route(\"Regist\")]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[HttpPost]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {exeReturn} Regist([FromBody] {entityName} {entityName.FirstCharToLower()})");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {exeReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "var user = this.GetAccessToken();");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (user != null && !string.IsNullOrWhiteSpace(user.ServerToken) && user.ServerToken == AppSettings.Current.ServerToken)");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"var tmp = db.Insert({entityName.FirstCharToLower()});");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result = tmp;");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result.Success(tmp);");
                }
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "else");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), "result.Error(\"Authorization header not found\");");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");

                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[Route(\"Update\")]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "[HttpPut]");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {exeReturn} Update([FromBody] {entityName} {entityName.FirstCharToLower()})");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {exeReturn}();");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "var user = this.GetAccessToken();");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (user != null && !string.IsNullOrWhiteSpace(user.ServerToken) && user.ServerToken == AppSettings.Current.ServerToken)");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"var tmp = db.Update({entityName.FirstCharToLower()});");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result = tmp;");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result.Success(tmp);");
                }
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
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (user != null && !string.IsNullOrWhiteSpace(user.ServerToken) && user.ServerToken == AppSettings.Current.ServerToken)");
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
                builder.AppendTabStringLine((IsNamespace ? 5 : 4), "var tmp = db.Delete(idx);");
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result = tmp;");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 5 : 4), $"result.Success(tmp);");
                }
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

        public string CreateDefaultController(BindOption options, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (IsNamespace)
            {
                builder.AppendLine($"using Microsoft.AspNetCore.Mvc.Filters;");
                builder.AppendLine($"using Woose.Core;");
                builder.AppendLine($"using Woose.Data;");
                builder.AppendLine($"using Woose.API;");
                builder.AppendEmptyLine();
                builder.AppendLine($"namespace {options.ProjectName}");
                builder.AppendLine("{");
            }

            builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class DefaultController : BaseController");
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public DefaultController(IContext context, ICryptoHandler crypto, IConfiguration config) : base(context,crypto,config)");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "//전역으로 적용할 공통사항은 여기에 기술하세요.");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "public override void OnActionExecuting(ActionExecutingContext context)");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "base.OnActionExecuting(context);");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "public override void OnActionExecuted(ActionExecutedContext context)");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "base.OnActionExecuted(context);");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "protected override User? GetAccessToken()");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "//인증수단 변경은 여기서 진행하세요.");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "User? result = base.GetAccessToken();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "if (result == null && this.access_token == \"test-token\")");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), "result = new User();");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), "result.Id = \"admin\";");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), "result.Name = \"관리자\";");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), "result.ServerToken = AppSettings.Current.ServerToken;");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "}");
            if (IsNamespace)
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        public string CreateProcController(BindOption options, DbContext context, List<DbEntity> sps, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (IsNamespace)
            {
                builder.AppendLine("using Microsoft.AspNetCore.Mvc;");
                builder.AppendLine("using Woose.Core;");
                builder.AppendLine("using Woose.Data;");
                builder.AppendLine("using Woose.API;");
                builder.AppendEmptyLine();
                builder.AppendLine($"namespace {options.ProjectName}");
                builder.AppendLine("{");
            }

            builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"[Route(\"api/Proc\")]");
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "[ApiController]");
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class {options.MethodName}ProcController : DefaultController");
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"protected I{options.MethodName}Repository db;");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {options.MethodName}ProcController(IContext context, ICryptoHandler crypto, IConfiguration config, I{options.MethodName}Repository _db) : base(context,crypto,config)");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "this.db = _db;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();
            if (sps != null && sps.Count > 0)
            {
                foreach (var sp in sps)
                {
                    if (!CodeHelper.ContainEntitySaveSP(sp.name, out string entity))
                    {
                        builder.AppendLine(CreateSPControllerItem(options, context, sp, IsNamespace));
                    }
                }
            }
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "}");

            if (IsNamespace)
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }



        public string CreateParameter(BindOption options, DbEntity sp, List<SPEntity> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (properties != null && properties.Count > 0)
            {
                if (IsNamespace)
                {
                    builder.AppendLine($"using Woose.Core;");
                    builder.AppendLine($"using Woose.Data;");
                    builder.AppendLine($"using Woose.API;");
                    builder.AppendEmptyLine();
                    builder.AppendLine($"namespace {options.ProjectName}");
                    builder.AppendLine("{");
                }

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class Input{GetNameFromSP(sp.name)}Parameter : IParameter");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");

                foreach (var item in properties)
                {
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
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public Input{GetNameFromSP(sp.name)}Parameter()");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
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

        public string CreateParameter(BindOption options, DbEntity sp, List<SpOutput> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (properties != null && properties.Count > 0)
            {
                if (IsNamespace)
                {
                    builder.AppendLine($"using Woose.Core;");
                    builder.AppendLine($"using Woose.Data;");
                    builder.AppendLine($"using Woose.API;");
                    builder.AppendEmptyLine();
                    builder.AppendLine($"namespace {options.ProjectName}");
                    builder.AppendLine("{");
                }

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class Output{GetNameFromSP(sp.name)}Parameter : IParameter");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");

                foreach (var item in properties)
                {
                    builder.AppendTabString((IsNamespace ? 2 : 1), $"public {item.ObjectType} {item.name}");
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
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public Output{GetNameFromSP(sp.name)}Parameter()");
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
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

        public string CreateAbstract(BindOption options, DbContext context, List<DbEntity> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (properties != null && properties.Count > 0)
            {
                if (IsNamespace)
                {
                    builder.AppendLine($"using Woose.Core;");
                    builder.AppendLine($"using Woose.Data;");
                    builder.AppendLine($"using Woose.API;");
                    builder.AppendEmptyLine();
                    builder.AppendLine($"namespace {options.ProjectName}");
                    builder.AppendLine("{");
                }

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public interface I{options.MethodName}Repository : IRepository");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
                
                if (properties != null && properties.Count > 0)
                {
                    int num = 0;
                    foreach (var sp in properties)
                    {
                        if (num > 0)
                        {
                            builder.AppendEmptyLine();
                        }
                        builder.AppendLine(CreateSPAbstractItem(options, context, sp, IsNamespace));
                        num++;
                    }
                }

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "}");

                if (IsNamespace)
                {
                    builder.AppendLine("}");
                }
            }

            return builder.ToString();
        }

        public string CreateAbstract(BindOption options, DbEntity entity, List<DbTableInfo> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            DbTableInfo? primaryKey = properties.Where(x => x.is_identity).FirstOrDefault();

            string exeReturn = string.Empty;

            if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
            {
                exeReturn = "ReturnValue";
            }
            else
            {
                exeReturn = "ExecuteResult";
            }

            if (properties != null && properties.Count > 0)
            {
                if (IsNamespace)
                {
                    builder.AppendLine($"using Woose.Core;");
                    builder.AppendLine($"using Woose.Data;");
                    builder.AppendLine($"using Woose.API;");
                    builder.AppendEmptyLine();
                    builder.AppendLine($"namespace {options.ProjectName}");
                    builder.AppendLine("{");
                }

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public interface I{entity.name}Repository : I{options.MethodName}Repository");
                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
                if (primaryKey != null && !string.IsNullOrWhiteSpace(primaryKey.Name))
                {
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"{entity.name} Single(long {primaryKey.Name.FirstCharToLower()});");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"{entity.name} Single(long idx);");
                }
                builder.AppendEmptyLine();

                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"List<{entity.name}> Select(string whereStr);");
                builder.AppendEmptyLine();

                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"List<{entity.name}> List(PagingParameter paramData);");
                builder.AppendEmptyLine();

                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"int Count(PagingParameter paramData);");
                builder.AppendEmptyLine();

                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"{exeReturn} Insert({entity.name} {entity.name.FirstCharToLower()});");
                builder.AppendEmptyLine();

                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"{exeReturn} Update({entity.name} {entity.name.FirstCharToLower()});");
                builder.AppendEmptyLine();

                if (primaryKey != null && !string.IsNullOrWhiteSpace(primaryKey.Name))
                {
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"{exeReturn} Delete(long {primaryKey.Name.FirstCharToLower()});");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"{exeReturn} Delete(long idx);");
                }

                builder.AppendTabStringLine((IsNamespace ? 1 : 0), "}");

                if (IsNamespace)
                {
                    builder.AppendLine("}");
                }
            }

            return builder.ToString();
        }

        public string CreateDefaultRepository(BindOption options, DbContext context, List<DbEntity> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            if (IsNamespace)
            {
                builder.AppendLine("using System.Data;");
                builder.AppendLine("using Woose.Core;");
                builder.AppendLine("using Woose.Data;");
                builder.AppendLine("using Woose.API;");
                builder.AppendEmptyLine();
                builder.AppendLine($"namespace {options.ProjectName}");
                builder.AppendLine("{");
            }

            builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class {options.MethodName}Repository : BaseRepository, I{options.MethodName}Repository");
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {options.MethodName}Repository(IContext context) : base(context)");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();
            if (properties != null && properties.Count > 0)
            {
                foreach (var sp in properties)
                {
                    builder.AppendLine(CreateSPItem(options, context, sp, IsNamespace));
                }
            }
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "}");

            if (IsNamespace)
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        public string CreateRepository(BindOption options, DbEntity entity, List<DbTableInfo> properties, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            DbTableInfo? primaryKey = properties.Where(x => x.is_identity).FirstOrDefault();
            if (primaryKey == null)
            {
                primaryKey = properties.Where(x => x.ColumnType.Equals("bigint", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = properties.Where(x => x.IsNumber).FirstOrDefault();
                }
            }

            string exeReturn = string.Empty;
            string exeReturnMethod = string.Empty;

            if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
            {
                exeReturn = "ReturnValue";
                exeReturnMethod = "ExecuteReturnValue";
            }
            else
            {
                exeReturn = "ExecuteResult";
                exeReturnMethod = "ExecuteResult";
            }

            if (IsNamespace)
            {
                builder.AppendLine("using System.Data;");
                builder.AppendLine("using Woose.Core;");
                builder.AppendLine("using Woose.Data;");
                builder.AppendLine("using Woose.API;");
                builder.AppendEmptyLine();
                builder.AppendLine($"namespace {options.ProjectName}");
                builder.AppendLine("{");
            }

            builder.AppendTabStringLine((IsNamespace ? 1 : 0), $"public class {entity.name}Repository : {options.MethodName}Repository, I{entity.name}Repository");
            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "{");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {entity.name}Repository(IContext context) : base(context)");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();
            if (primaryKey != null && !string.IsNullOrWhiteSpace(primaryKey.Name))
            {
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {entity.name} Single(long {primaryKey.Name.FirstCharToLower()})");
            }
            else
            {
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {entity.name} Single(long idx)");
            }
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {entity.name}();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var db = context.getConnection())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var cmd = db.CreateCommand())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            if (primaryKey != null && !string.IsNullOrWhiteSpace(primaryKey.Name))
            {
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On<{entity.name}>().Select(1).Where(\"{primaryKey.Name}\", {primaryKey.Name.FirstCharToLower()}).Set();");
            }
            else
            {
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On<{entity.name}>().Select(1).Where(\"idx\", idx).Set();");
            }
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.ExecuteEntity<{entity.name}>();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();

            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public List<{entity.name}> Select(string whereStr = \"\")");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new List<{entity.name}>();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var db = context.getConnection())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var cmd = db.CreateCommand())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On<{entity.name}>().Select().Where(whereStr).Set();");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.ExecuteEntities<{entity.name}>();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();

            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public List<{entity.name}> List(PagingParameter paramData)");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new List<{entity.name}>();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var db = context.getConnection())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var cmd = db.CreateCommand())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On<{entity.name}>().Paging(10, paramData.CurPage).Where(paramData.ToWhereString()).Set();");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.ExecuteEntities<{entity.name}>();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();

            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public int Count(PagingParameter paramData)");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "int result = 0;");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var db = context.getConnection())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var cmd = db.CreateCommand())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On<{entity.name}>().Count().Where(paramData.ToWhereString()).Set();");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.ExecuteCount();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();

            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {exeReturn} Insert({entity.name} {entity.name.FirstCharToLower()})");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {exeReturn}();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var db = context.getConnection())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var cmd = db.CreateCommand())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On<{entity.name}>({entity.name.FirstCharToLower()}).Insert().Try().Set();");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.{exeReturnMethod}();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();

            builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {exeReturn} Update({entity.name} {entity.name.FirstCharToLower()})");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {exeReturn}();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var db = context.getConnection())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var cmd = db.CreateCommand())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On<{entity.name}>({entity.name.FirstCharToLower()}).Update().Try().Where(\"{primaryKey?.Name ?? "idx"}\", {entity.name.FirstCharToLower()}.{primaryKey?.Name ?? "idx"}).Set();");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.{exeReturnMethod}();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");
            builder.AppendEmptyLine();

            if (primaryKey != null && !string.IsNullOrWhiteSpace(primaryKey.Name))
            {
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {exeReturn} Delete(long {primaryKey.Name.FirstCharToLower()})");
            }
            else
            {
                builder.AppendTabStringLine((IsNamespace ? 2 : 1), $"public {exeReturn} Delete(long idx)");
            }
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {exeReturn}();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var db = context.getConnection())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var cmd = db.CreateCommand())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            if (primaryKey != null && !string.IsNullOrWhiteSpace(primaryKey.Name))
            {
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On<{entity.name}>().Delete().Try().Where(\"{primaryKey.Name}\", {primaryKey.Name.FirstCharToLower()}).Set();");
            }
            else
            {
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On<{entity.name}>().Delete().Try().Where(\"idx\",idx).Set();");
            }
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.{exeReturnMethod}();");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");

            builder.AppendTabStringLine((IsNamespace ? 1 : 0), "}");

            if (IsNamespace)
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        //Repository
        private string CreateSPItem(BindOption options, DbContext context, DbEntity property, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            string exeReturn = string.Empty;
            int num = 0;
            bool isAnotherModel = false;
            List<SPEntity> inputs = new List<SPEntity>();
            List<SpOutput> outputs = new List<SpOutput>();
            DbEntity? output = null;

            using (var rep = new SqlServerRepository(context))
            {
                inputs = rep.GetSpProperties(property.name);
                outputs = rep.GetSpOutput(property.name);
                output = rep.Find(outputs);
            }

            if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
            {
                exeReturn = "ReturnValue";
            }
            else
            {
                exeReturn = "ExecuteResult";
            }

            
            if (outputs != null && outputs.Count > 0)
            {
                if (outputs.Where(x => x.name == "IsError").Count() > 0)
                {
                    exeReturn = "ExecuteResult";
                }
                else
                {
                    if (output != null)
                    {
                        exeReturn = output.name;
                    }
                    else
                    {
                        exeReturn = $"Output{GetNameFromSP(property.name)}Parameter";
                    }
                    isAnotherModel = true;
                }
            }

            builder.AppendTabString((IsNamespace ? 2 : 1), $"public {((isAnotherModel) ? $"List<{exeReturn}>" : exeReturn)}? {GetNameFromSP(property.name)}(");
            if (options.IsNoModel)
            {
                num = 0;
                foreach (var input in inputs)
                {
                    if (num > 0) builder.Append(",");
                    builder.Append($"{input.ObjectType} {input.Name.FirstCharToLower()}");
                    num++;
                }
            }
            else
            {
                builder.Append($"Input{GetNameFromSP(property.name)}Parameter input{GetNameFromSP(property.name)}");
            }
            builder.AppendLine(")");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "{");
            if (isAnotherModel)
            {
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new List<{exeReturn}>();");
            }
            else
            {
                builder.AppendTabStringLine((IsNamespace ? 3 : 2), $"var result = new {exeReturn}();");
            }
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var db = context.getConnection())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "using (var cmd = db.CreateCommand())");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "{");
            builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"cmd.On(\"{property.name}\").Set();");
            foreach (var input in inputs)
            {
                builder.AppendTabString((IsNamespace ? 4 : 3), $"cmd.Parameters.Set(\"{input.name}\", SqlDbType.{input.CsType}, ");
                if (options.IsNoModel)
                {
                    builder.Append(input.name.FirstCharToLower());
                }
                else
                {
                    builder.Append($"input{GetNameFromSP(property.name)}.{input.name.Replace("@","")}");
                }
                if (input.IsSize)
                {
                    if (input.CsType.Equals("NVarChar", StringComparison.OrdinalIgnoreCase) || input.CsType.Equals("VarChar", StringComparison.OrdinalIgnoreCase))
                    {
                        if (input.max_length < 1)
                        {
                            builder.Append($", -1");
                        }
                        else
                        {
                            builder.Append($", {input.max_length}");
                        }
                    }
                    else
                    {
                        builder.Append($", {input.max_length}");
                    }
                    
                }
                builder.AppendLine(");");
            }
            if (isAnotherModel)
            {
                builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.ExecuteEntities<{exeReturn}>();");
            }
            else
            {
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.ExecuteReturnValue();");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? 4 : 3), $"result = cmd.ExecuteResult();");
                }
            }
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "}");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "");
            builder.AppendTabStringLine((IsNamespace ? 3 : 2), "return result;");
            builder.AppendTabStringLine((IsNamespace ? 2 : 1), "}");

            return builder.ToString();
        }

        //Controller
        private string CreateSPControllerItem(BindOption options, DbContext context, DbEntity property, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            string exeReturn = string.Empty;
            string methodType = "HttpPost";
            string formType = "FromBody";
            int num = 0;
            int startindex = 2;
            bool isAnotherModel = false;
            List<SPEntity> inputs = new List<SPEntity>();
            List<SpOutput> outputs = new List<SpOutput>();
            DbEntity? output = null;

            using (var rep = new SqlServerRepository(context))
            {
                inputs = rep.GetSpProperties(property.name);
                outputs = rep.GetSpOutput(property.name);
                output = rep.Find(outputs);
            }

            if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
            {
                exeReturn = "ReturnValue";
            }
            else
            {
                exeReturn = "ExecuteResult";
            }


            if (outputs != null && outputs.Count > 0)
            {
                if (outputs.Where(x => x.name == "IsError").Count() > 0)
                {
                    exeReturn = "ExecuteResult";
                }
                else
                {
                    if (output != null)
                    {
                        exeReturn = output.name;
                    }
                    else
                    {
                        exeReturn = $"Output{GetNameFromSP(property.name)}Parameter";
                    }
                    methodType = "HttpGet";
                    formType = "FromQuery";
                    isAnotherModel = true;
                }
            }

            builder.AppendTabStringLine((IsNamespace ? startindex : (startindex + 1)), $"[Route(\"{GetNameFromSP(property.name).AddSlashBeforeUppercase()}\")]");
            builder.AppendTabStringLine((IsNamespace ? startindex : (startindex + 1)), $"[{methodType}]");
            builder.AppendTabString((IsNamespace ? startindex : (startindex + 1)), $"public {((isAnotherModel) ? $"List<{exeReturn}>" : exeReturn)}? {GetNameFromSP(property.name)}(");
            if (options.IsNoModel)
            {
                num = 0;
                foreach (var input in inputs)
                {
                    if (num > 0) builder.Append(",");
                    builder.Append($"[{formType}] {input.ObjectType} {input.Name.FirstCharToLower()}");
                    num++;
                }
            }
            else
            {
                builder.Append($"[{formType}] Input{GetNameFromSP(property.name)}Parameter input{GetNameFromSP(property.name)}");
            }
            builder.AppendLine(")");
            builder.AppendTabStringLine((IsNamespace ? startindex : (startindex + 1)), "{");

            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), $"var result = db.{GetNameFromSP(property.name)}(input{GetNameFromSP(property.name)});");
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), $"return result;");

            builder.AppendTabStringLine((IsNamespace ? startindex : (startindex + 1)), "}");

            return builder.ToString();
        }

        private string CreateControllerSP(BindOption options, DbContext context, DbEntity property, bool IsNamespace = false, int startindex = 2, string RouteName = "")
        {
            StringBuilder builder = new StringBuilder(200);

            string exeReturn = string.Empty;
            string methodType = "HttpPost";
            string formType = "FromBody";
            int num = 0;
            bool isAnotherModel = false;
            List<SPEntity> inputs = new List<SPEntity>();
            List<SpOutput> outputs = new List<SpOutput>();
            DbEntity? output = null;

            using (var rep = new SqlServerRepository(context))
            {
                inputs = rep.GetSpProperties(property.name);
                outputs = rep.GetSpOutput(property.name);
                output = rep.Find(outputs);
            }

            if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
            {
                exeReturn = "ReturnValue";
            }
            else
            {
                exeReturn = "ApiResult<ExecuteResult>";
            }


            if (outputs != null && outputs.Count > 0)
            {
                if (outputs.Where(x => x.name == "IsError").Count() > 0)
                {
                    exeReturn = "ApiResult<ExecuteResult>";
                }
                else
                {
                    if (output != null)
                    {
                        exeReturn = output.name;
                    }
                    else
                    {
                        exeReturn = $"Output{GetNameFromSP(property.name)}Parameter";
                    }
                    methodType = "HttpGet";
                    formType = "FromQuery";
                    isAnotherModel = true;
                }
            }

            if (string.IsNullOrWhiteSpace(RouteName))
            {
                builder.AppendTabStringLine((IsNamespace ? startindex : (startindex + 1)), $"[Route(\"{GetNameFromSP(property.name).AddSlashBeforeUppercase()}\")]");
            }
            else
            {
                builder.AppendTabStringLine((IsNamespace ? startindex : (startindex + 1)), $"[Route(\"{RouteName}\")]");
            }
            builder.AppendTabStringLine((IsNamespace ? startindex : (startindex + 1)), $"[{methodType}]");
            builder.AppendTabString((IsNamespace ? startindex : (startindex + 1)), $"public {((isAnotherModel) ? $"List<{exeReturn}>" : exeReturn)} {GetNameFromSP(property.name)}(");
            if (options.IsNoModel)
            {
                num = 0;
                foreach (var input in inputs)
                {
                    if (num > 0) builder.Append(",");
                    builder.Append($"[{formType}] {input.ObjectType} {input.Name.FirstCharToLower()}");
                    num++;
                }
            }
            else
            {
                builder.Append($"[{formType}] Input{GetNameFromSP(property.name)}Parameter input{GetNameFromSP(property.name)}");
            }
            builder.AppendLine(")");
            builder.AppendTabStringLine((IsNamespace ? startindex : (startindex + 1)), "{");
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), $"var result = new {exeReturn}();");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), $"var user = this.GetAccessToken();");
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), $"if (user != null && !string.IsNullOrWhiteSpace(user.ServerToken) && user.ServerToken == AppSettings.Current.ServerToken)");
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), "{");

            builder.AppendTabStringLine((IsNamespace ? (startindex + 2) : (startindex + 3)), $"var tmp = db.{GetNameFromSP(property.name)}(input{GetNameFromSP(property.name)});");
            if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
            {
                builder.AppendTabStringLine((IsNamespace ? (startindex + 2) : (startindex + 3)), $"result.Success(1, tmp);");
            }
            else
            {
                if (isAnotherModel)
                {
                    builder.AppendTabStringLine((IsNamespace ? (startindex + 2) : (startindex + 3)), $"result = tmp;");
                }
                else
                {
                    builder.AppendTabStringLine((IsNamespace ? (startindex + 2) : (startindex + 3)), $"result = tmp?.ToResult();");
                }
                
            }
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), "}");
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), $"else");
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), "{");
            builder.AppendTabStringLine((IsNamespace ? (startindex + 2) : (startindex + 3)), "result.Error(\"Authorization header not found\");");
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), "}");
            builder.AppendEmptyLine();
            builder.AppendTabStringLine((IsNamespace ? (startindex + 1) : (startindex + 2)), "return result;");
            builder.AppendTabStringLine((IsNamespace ? startindex : (startindex + 1)), "}");

            return builder.ToString();
        }


        private string CreateSPAbstractItem(BindOption options, DbContext context, DbEntity property, bool IsNamespace = false)
        {
            StringBuilder builder = new StringBuilder(200);

            string exeReturn = string.Empty;
            int num = 0;
            bool isAnotherModel = false;
            List<SPEntity> inputs = new List<SPEntity>();
            List<SpOutput> outputs = new List<SpOutput>();
            DbEntity? output = null;

            using (var rep = new SqlServerRepository(context))
            {
                inputs = rep.GetSpProperties(property.name);
                outputs = rep.GetSpOutput(property.name);
                output = rep.Find(outputs);
            }

            if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
            {
                exeReturn = "ReturnValue";
            }
            else
            {
                exeReturn = "ExecuteResult";
            }

            if (outputs != null && outputs.Count > 0)
            {
                if (outputs.Where(x => x.name == "IsError").Count() > 0)
                {
                    exeReturn = "ExecuteResult";
                }
                else
                {
                    if (output != null)
                    {
                        exeReturn = output.name;
                    }
                    else
                    {
                        exeReturn = $"Output{GetNameFromSP(property.name)}Parameter";
                    }
                    isAnotherModel = true;
                }
            }

            builder.AppendTabString((IsNamespace ? 2 : 1), $"{((isAnotherModel) ? $"List<{exeReturn}>" : exeReturn)}? {GetNameFromSP(property.name)}(");
            if (options.IsNoModel)
            {
                num = 0;
                foreach (var input in inputs)
                {
                    if (num > 0) builder.Append(",");
                    builder.Append($"{input.ObjectType} {input.Name.FirstCharToLower()}");
                    num++;
                }
            }
            else
            {
                builder.Append($"Input{GetNameFromSP(property.name)}Parameter input{GetNameFromSP(property.name)}");
            }
            builder.AppendLine(");");

            return builder.ToString();
        }

        public string CreateEntity(BindOption options, List<DbTableInfo> properties)
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

        public string? CreateProgram(BindOption option, List<DbEntity> properties)
        {
            StringBuilder builder = new StringBuilder(200);

            builder.AppendLine($"using {option.ProjectName};");
            builder.AppendLine("using Woose.API;");
            builder.AppendLine("using Woose.Data;");
            builder.AppendLine("using Microsoft.AspNetCore.Authentication.JwtBearer;");
            builder.AppendLine("using Microsoft.IdentityModel.Tokens;");
            builder.AppendLine("using Microsoft.OpenApi.Models;");
            builder.AppendLine("using System.Reflection;");
            builder.AppendLine("using System.Text;");
            builder.AppendLine("");
            builder.AppendLine("var builder = WebApplication.CreateBuilder(args);");
            builder.AppendLine("builder.Services.AddControllers();");
            builder.AppendLine("builder.Services.AddEndpointsApiExplorer();");
            builder.AppendLine("builder.Services.AddSwaggerGen(c =>");
            builder.AppendLine("{");
            builder.AppendLine("    c.SwaggerDoc(\"v1\", new OpenApiInfo { Title = \"" + option.ProjectName.Replace(".", " ") + " Api\", Version = \"v1\" });");
            builder.AppendLine("    c.AddSecurityDefinition(\"Bearer\", new OpenApiSecurityScheme");
            builder.AppendLine("    {");
            builder.AppendLine("        Description = @\"로그인을 제외하고 모든 API 호출에 필수값입니다.\",");
            builder.AppendLine("        Name = \"access_token\",");
            builder.AppendLine("        In = ParameterLocation.Header,");
            builder.AppendLine("        Type = SecuritySchemeType.ApiKey,");
            builder.AppendLine("        Scheme = \"Bearer\"");
            builder.AppendLine("    });");
            builder.AppendLine("");
            builder.AppendLine("    c.AddSecurityRequirement(new OpenApiSecurityRequirement {");
            builder.AppendLine("    {");
            builder.AppendLine("        new OpenApiSecurityScheme");
            builder.AppendLine("        {");
            builder.AppendLine("        Reference = new OpenApiReference");
            builder.AppendLine("        {");
            builder.AppendLine("            Type = ReferenceType.SecurityScheme,");
            builder.AppendLine("            Id = \"Bearer\"");
            builder.AppendLine("        }");
            builder.AppendLine("        },");
            builder.AppendLine("        new string[] { }");
            builder.AppendLine("    }");
            builder.AppendLine("    });");
            builder.AppendLine("");
            builder.AppendLine("    // XML 주석 파일 경로 설정");
            builder.AppendLine("    var xmlFile = $\"{Assembly.GetExecutingAssembly().GetName().Name}.xml\";");
            builder.AppendLine("    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);");
            builder.AppendLine("    c.IncludeXmlComments(xmlPath);");
            builder.AppendLine("});");
            builder.AppendLine("");
            builder.AppendLine("builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)");
            builder.AppendLine(".AddJwtBearer(options =>");
            builder.AppendLine("{");
            builder.AppendLine("    options.TokenValidationParameters = new TokenValidationParameters");
            builder.AppendLine("    {");
            builder.AppendLine("        ValidateIssuerSigningKey = true,");
            builder.AppendLine("        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(WooseSecret.SecretKey)),");
            builder.AppendLine("        ValidateIssuer = false,");
            builder.AppendLine("        ValidateAudience = false,");
            builder.AppendLine("        ClockSkew = TimeSpan.Zero");
            builder.AppendLine("    };");
            builder.AppendLine("});");
            builder.AppendLine("");
            builder.AppendLine("string connectionString = builder.Configuration.GetSection(\"Database\").GetSection(\"ConnectionString\").Value!;");
            builder.AppendLine("string[] corsPolicies = new string[] { \"127.0.0.1\", \"localhost:8080\" };");
            builder.AppendLine("");
            builder.AppendLine("builder.Services.AddCors(options =>");
            builder.AppendLine("{");
            builder.AppendLine("    options.AddDefaultPolicy(");
            builder.AppendLine("        builder =>");
            builder.AppendLine("        {");
            builder.AppendLine("            builder");
            builder.AppendLine("                .WithOrigins(corsPolicies)");
            builder.AppendLine("                .AllowAnyHeader()");
            builder.AppendLine("                .AllowAnyMethod()");
            builder.AppendLine("                .AllowCredentials();");
            builder.AppendLine("        });");
            builder.AppendLine("});");
            builder.AppendLine("");
            builder.AppendLine("builder.Services.AddSingleton<IContext>(provider => new DbContext(connectionString));");

            builder.AppendLine($"builder.Services.AddSingleton<I{option.MethodName}Repository, {option.MethodName}Repository>();");
            foreach(var table in properties)
            {
                builder.AppendLine($"builder.Services.AddSingleton<I{table.name}Repository, {table.name}Repository>();");
            }
            builder.AppendLine("builder.Services.AddSingleton<ICryptoHandler, CryptoHandler>();");
            builder.AppendLine("");
            builder.AppendLine("var app = builder.Build();");
            builder.AppendLine("app.UseCors(builder =>");
            builder.AppendLine("{");
            builder.AppendLine("    builder");
            builder.AppendLine("    .WithOrigins(corsPolicies)");
            builder.AppendLine("    .AllowAnyMethod()");
            builder.AppendLine("    .AllowAnyHeader()");
            builder.AppendLine("    .AllowCredentials();");
            builder.AppendLine("});");
            builder.AppendLine("app.UseStaticFiles();");
            builder.AppendLine("app.UseSwagger();");
            builder.AppendLine("app.UseSwaggerUI();");
            builder.AppendLine("app.UseAuthorization();");
            builder.AppendLine("app.MapControllers();");
            builder.AppendLine("app.UseRouting();");
            builder.AppendLine("app.Run();");

            return builder.ToString();
        }

        public string GetFrom(string methodType)
        {
            switch (methodType.Trim().ToUpper())
            {
                case "HTTPGET":
                    return "FromQuery";
                case "HTTPPUT":
                    return "FromBody";
                case "HTTPDELETE":
                    return "FromBody";
                default:
                    return "FromBody";
            }
        }
    }
}
