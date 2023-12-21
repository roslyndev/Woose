using Newtonsoft.Json;
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



        public string NodePackgeJsonCreate(BindOption options)
        {
            StringBuilder builder = new StringBuilder(200);
            PackageJson json = new PackageJson();
            json.name = options.ProjectName.ToLower();
            json.version = "1.0.0";
            json.description = $"{options.ProjectName} with Woose Api Server";
            json.main = "index.js";
            json.scripts.start = "nodemon index.js";
            json.scripts.test = "mocha";
            json.repository.type = "git";
            json.keywords.Add(options.ProjectName);
            json.license = "ISC";
            // 각각의 라이브러리를 추가
            json.dependencies.axios = "^0.27.2";
            json.dependencies.bcryptjs = "^2.4.3";
            json.dependencies.cors = "^2.8.5";
            json.dependencies.crypto = "^1.0.1";
            json.dependencies.express = "^4.18.1";
            json.dependencies.expressFileupload = "^1.4.0";
            json.dependencies.jsonwebtoken = "^8.5.1";
            json.dependencies.moment = "^2.29.4";
            json.dependencies.multer = "^1.4.5-lts.1";
            json.dependencies.nodemailer = "^6.8.0";
            json.dependencies.nodemon = "^2.0.19";
            json.dependencies.sequelize = "^6.21.4";
            json.dependencies.sharp = "^0.30.7";
            json.dependencies.swaggerCli = "^4.0.4";
            json.dependencies.swaggerJsdoc = "^6.2.5";
            json.dependencies.swaggerUiExpress = "^4.5.0";
            json.dependencies.tedious = "^15.0.1";
            json.dependencies.uuid4 = "^2.0.3";
            json.dependencies.yamljs = "^0.3.0";
            json.devDependencies.swaggerUiExpress = "^4.1.3";
            json.devDependencies.yamljs = "^0.2.31";

            builder.Append(JsonConvert.SerializeObject(json, Formatting.Indented));
            return builder.ToString();
        }

        public string NodeConfigCreate(BindOption options)
        {
            StringBuilder builder = new StringBuilder(200);
            NodeJsConfig json = new NodeJsConfig();
            json.database.host = options.Database.GetHost();
            json.database.port = options.Database.GetPort();
            json.database.user = options.Database.GetID();
            json.database.password = options.Database.GetPassword();
            json.database.database = options.Database.GetDb();
            json.tokenKey = "access_token";
            json.port = 4000;
            json.secret = $"{options.ProjectName}  JWT key @ Woose";

            builder.Append("var config = ");
            builder.AppendLine(JsonConvert.SerializeObject(json, Formatting.Indented));
            builder.AppendLine("");
            builder.AppendLine("module.exports = config;");
            return builder.ToString();
        }

        public string NodeIndexJsCreate(BindOption options)
        {
            StringBuilder builder = new StringBuilder(200);
            builder.AppendLine("const express = require('express');");
            builder.AppendLine("const cors = require('cors');");
            builder.AppendLine("const path = require('path');");
            builder.AppendLine("const { swaggerUi } = require('./swagger');");
            builder.AppendLine("const apis = require('./routes');");
            builder.AppendLine("const YAML = require('yamljs');");
            builder.AppendLine("const globalConfig = require('./models/globalConfig');");
            builder.AppendLine("");
            builder.AppendLine("const app = express();");
            builder.AppendLine("const config = require('./config');");
            builder.AppendLine("");
            builder.AppendLine("app.use(cors());");
            builder.AppendLine("app.use(express.urlencoded({ extended: false })); ");
            builder.AppendLine("app.use(express.json());");
            builder.AppendLine("");
            builder.AppendLine("let imageURL = path.join(__dirname, 'uploads');");
            builder.AppendLine("app.use('/uploads', express.static(imageURL)); ");
            builder.AppendLine("");
            builder.AppendLine("const swaggerSpec = YAML.load(path.join(__dirname, './swagger/openapi.yaml'));");
            builder.AppendLine("");
            builder.AppendLine("let global = globalConfig.current.getInstance();");
            builder.AppendLine("global.set(\"root\", __dirname);");
            builder.AppendLine("");
            builder.AppendLine("app.use('/api', apis);");
            builder.AppendLine("app.use('/swagger', swaggerUi.serve, swaggerUi.setup(swaggerSpec));");
            builder.AppendLine("");
            builder.AppendLine("app.listen(config.port, () => {");
            builder.AppendTabStringLine(1, "console.log(`Server running successfully on ${config.port}`);");
            builder.AppendLine("});");
            return builder.ToString();
        }

        public string NodeGlobalJsCreate(BindOption options)
        {
            return @"
var global = { current : (function() {
    var instance;
    var name = 'global';
    var data = [];
    function init() {
      return {
        name: name,
        data: data,
        set: function(key, value) {
          data.push({ key : key, value : value});
        },
        get: function(key) {
            let result = null;
            for(let i = 0; i < data.length; i++) {
                if (data[i].key === key) {
                    result = data[i].value;
                    break;
                }
            }

            return result;
        }
      };
    }
    return {
      getInstance: function() {
        if (!instance) {
          instance = init();
        }
        return instance;
      }
    }
  })()
};

module.exports = global;
                    ";
        }

        public string NodeSwaggerJsCreate(BindOption options)
        {
            StringBuilder builder = new StringBuilder(200);
            builder.Append(@"
const swaggerUi = require(""swagger-ui-express"")
const swaggereJsdoc = require(""swagger-jsdoc"")

const config = require('./config');

const options = {
  swaggerDefinition: {
    openapi: ""3.0.0"",
    info: {
      version: ""1.0.0"",
      title: """);
            builder.Append(options.ProjectName);
            builder.Append(@" v1.0 swagger"",
      description:
        ""Documentation service to help with API development"",
    },
    components: {
      securitySchemes: {
        bearer : {
          type: ""http"",
          name: ""access_token"",
          scheme: ""bearer"",
          in:""header""
        }
      }
    },
    securityDefinitions:{
      bearer:{
        type: ""http"",
        name: ""access_token"",
        scheme: ""bearer"",
        in:""header""
      }
      },
    security: [
      {
        bearer: []
      },
    ],
    servers: [
      {
        url: `localhost:4000`,
      },
    ],
  },
  apis: [
    `./routes/*.js`, 
  ],
}
const specs = swaggereJsdoc(options)

module.exports = { swaggerUi, specs }");
            return builder.ToString();
        }
    }
}
