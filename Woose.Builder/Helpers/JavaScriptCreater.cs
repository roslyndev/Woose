using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime;
using System.Text;
using Woose.Data;

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
                builder.AppendTabLine(1, "let result = new ApiResult();");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "try {");
                builder.AppendTab(2, $"const [results, metadata] = await sequelize.query('EXEC {spName} ");
                num = 0;
                foreach(var item in info)
                {
                    if (num > 0) { builder.Append(", "); }
                    builder.Append($"{item.name} = :{item.name.Replace("@", "")}");
                    num++;
                }
                builder.AppendLine("', {");
                builder.AppendTabLine(3, "replacements: {");
                num = 0;
                foreach (var item in info)
                {
                    if (num > 0) 
                    { 
                        builder.AppendTab(4, ","); 
                    }
                    else
                    {
                        builder.AppendTab(4, "");
                    }
                    builder.AppendTabLine(1, $"{item.name.Replace("@", "")}: {item.name.Replace("@", "").FirstCharToLower()}");
                    num++;
                }
                builder.AppendTabLine(3, "}");
                builder.AppendTabLine(2, "");
                builder.AppendTabLine(3, "if (metadata && metadata.rowsAffected && metadata.rowsAffected[0] > 0) {");
                builder.AppendTabLine(4, "result.Success(metadata.rowsAffected[0]);");
                builder.AppendTabLine(3, "} else {");
                builder.AppendTabLine(4, "result.Error('Update Fail');");
                builder.AppendTabLine(3, "}");
                builder.AppendTabLine(1, "} catch (e) {");
                builder.AppendTabLine(2, "result.Error(e.message);");
                builder.AppendTabLine(1, "} finally {");
                builder.AppendTabLine(2, "return result;");
                builder.AppendTabLine(1, "}");
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
                builder.AppendTabLine(1, "const attributes = {");
                foreach(var item in info)
                {
                    if (item.is_identity)
                    {
                        builder.AppendTab(2, $"{item.Name}: ");
                        builder.Append("{");
                        builder.Append($" type: DataTypes.{item.SequelizeMsSqlType}");
                        if (item.IsSize)
                        {
                            if (item.max_length > 0)
                            {
                                builder.Append($"({item.max_length})");
                            }
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
                        builder.AppendTab(2, $"{item.Name}: ");
                        builder.Append("{");
                        if (item.IsDate)
                        {
                            builder.Append($" type: DataTypes.DATE");
                        }
                        else
                        {
                            builder.Append($" type: DataTypes.{item.SequelizeMsSqlType}");
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
                builder.AppendTabLine(1, "};");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "const options = {");
                builder.AppendTabLine(2, $"tableName: \"{info[0].TableName}\",");
                if (info.Where(x => x.Name.Equals("Password", StringComparison.OrdinalIgnoreCase)).Count() > 0)
                {
                    builder.AppendTabLine(2, "defaultScope: {");
                    builder.AppendTabLine(3, "attributes: { exclude: ['Password'] }");
                    builder.AppendTabLine(2, "},");
                    builder.AppendTabLine(2, "scopes: {");
                    builder.AppendTabLine(3, "withHash: { attributes: {}, }");
                    builder.AppendTabLine(2, "},");
                }
                if (primaryKey != null)
                {
                    if (!(primaryKey.IsNumber && primaryKey.is_identity))
                    {
                        builder.AppendTabLine(2, "freezeTableName: true,");
                    }
                }
                builder.AppendTabLine(2, "timestamps: false");
                builder.AppendTabLine(1, "};");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, $"return sequelize.define('{entityName}', attributes, options);");
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
                builder.AppendTabLine(1, "let result = new ApiResult();");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "try {");
                builder.AppendTab(2, $"if ({entityName.FirstCharToLower()} !== null && {entityName.FirstCharToLower()} !== undefined)");
                builder.AppendLine(" {");
                builder.AppendTab(3, $"let target = await db.{entityName.FirstCharToLower()}.findOne(");
                builder.AppendLine("{");
                builder.AppendTab(4, "where : { ");
                builder.Append($"{primaryKey?.ColumnName} : {entityName.FirstCharToLower()}.{primaryKey?.ColumnName}");
                builder.AppendLine(" }");
                builder.AppendTabLine(3, "});");
                builder.AppendEmptyLine();
                builder.AppendTab(3, $"if (target !== null && target !== undefined && target.{primaryKey?.ColumnName} > 0)");
                builder.AppendLine(" {");
                foreach (var item in info.Where(x => x.ColumnName != primaryKey?.ColumnName))
                {
                    builder.AppendTabLine(4, $"target.{item.ColumnName} = {entityName.FirstCharToLower()}.{item.ColumnName}");
                }
                builder.AppendTabLine(4, "await target.save();");
                builder.AppendEmptyLine();
                builder.AppendTab(4, $"if (target.{primaryKey?.ColumnName} > 0) ");
                builder.AppendLine("{");
                builder.AppendTabLine(5, $"result.Success(target.{primaryKey?.ColumnName});");
                builder.AppendTabLine(4, "} else {");
                builder.AppendTabLine(5, "result.Error(\"수정에 실패하였습니다.\");");
                builder.AppendTabLine(4, "}");
                builder.AppendTabLine(3, "} else {");
                builder.AppendTabLine(4, $"let target = new db.{entityName.FirstCharToLower()}();");
                builder.AppendEmptyLine();
                foreach(var item in info.Where(x => x.ColumnName != primaryKey?.ColumnName))
                {
                    builder.AppendTabLine(4, $"target.{item.ColumnName} = {entityName.FirstCharToLower()}.{item.ColumnName}");
                }
                builder.AppendTabLine(4, "await target.save();");
                builder.AppendEmptyLine();
                builder.AppendTab(4, $"if (target.{primaryKey?.ColumnName} > 0) ");
                builder.AppendLine("{");
                builder.AppendTabLine(5, $"result.Success(target.{primaryKey?.ColumnName});");
                builder.AppendTabLine(4, "} else {");
                builder.AppendTabLine(5, "result.Error(\"추가에 실패하였습니다.\");");
                builder.AppendTabLine(4, "}");
                builder.AppendTabLine(3, "}");
                
                
                

                builder.AppendTabLine(2, "} else {");
                builder.AppendTabLine(3, "result.Error(\"잘못된 접근입니다.\");");
                builder.AppendTabLine(2, "}");
                builder.AppendTabLine(1, "} catch (e) {");
                builder.AppendTabLine(2, "result.Error(e.message);");
                builder.AppendTabLine(1, "} finally {");
                builder.AppendTabLine(2, "return result;");
                builder.AppendTabLine(1, "}");
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
            json.dependencies.firebaseAdmin = "^11.6.0";
            json.dependencies.swaggerCli = "^4.0.4";
            json.dependencies.swaggerJsdoc = "^6.2.5";
            json.dependencies.swaggerUiExpress = "^4.5.0";
            json.dependencies.uuid4 = "^2.0.3";
            json.dependencies.yamljs = "^0.3.0";
            json.devDependencies.swaggerUiExpress = "^4.1.3";
            json.devDependencies.yamljs = "^0.2.31";

            switch (options.Database.DatabaseType)
            {
                case "SQL Server":
                case "MSSQL":
                    json.dependencies.sharp = "^0.30.7";
                    json.dependencies.tedious = "^15.0.1";
                    break;
                case "MySQL":
                    json.dependencies.mysql = "^2.18.1";
                    json.dependencies.mysql2 = "^3.2.3";
                    break;
                case "SQLite":
                    break;
            }

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
            builder.AppendLine("const fs = require('fs');");
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
            builder.AppendLine("const configYaml = YAML.parse(fs.readFileSync('./swagger/config.yaml', 'utf8'));");
            builder.AppendLine("const openapi = YAML.parse(fs.readFileSync('./swagger/openapi.yaml', 'utf8'));");
            builder.AppendLine("const components = YAML.parse(fs.readFileSync('./swagger/components.yaml', 'utf8'));");
            builder.AppendLine("const swaggerSpec = Object.assign({}, configYaml, openapi, components);");
            builder.AppendLine("");
            builder.AppendLine("let global = globalConfig.current.getInstance();");
            builder.AppendLine("global.set(\"root\", __dirname);");
            builder.AppendLine("");
            builder.AppendLine("app.use('/api', apis);");
            builder.AppendLine("app.use('/swagger', swaggerUi.serve, swaggerUi.setup(swaggerSpec));");
            builder.AppendLine("");
            builder.AppendLine("app.listen(config.port, () => {");
            builder.AppendTabLine(1, "console.log(`Server running successfully on ${config.port}`);");
            builder.AppendLine("});");
            return builder.ToString();
        }

        public string NodeGlobalJsCreate(BindOption options)
        {
            return @"
var globalConfig = { current : (function() {
    var instance;
    var name = 'global';
    var data = [];
    function init() {
      return {
        name: name,
        set: function(key, value) {
          data.push({ key : key, value : value });
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
        },
        remove: function(key) {
            for(let i = 0; i < data.length; i++) {
                if (data[i].key === key) {
                    data = data.splice(i, 1);
                    break;
                }
            }
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

module.exports = globalConfig;
                    ";
        }

        public string NodeSendMailJsCreate(BindOption options)
        {
            return @"const nodemailer = require('nodemailer');
const config = require('../config');

async function sendEmail({ to, subject, html, from = config.emailFrom }) {
    const transporter = nodemailer.createTransport(config.smtpOptions);
    await transporter.sendMail({ from, to, subject, html });
}

function sendEmailSync({ to, subject, html, from = config.emailFrom }) {
    const transporter = nodemailer.createTransport(config.smtpOptions);
    transporter.sendMail({ from, to, subject, html });
}

module.exports = { sendEmail, sendEmailSync };";
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

        public string NodeCryptoHelperCreate(BindOption options)
        {
            return @"
const jwt = require('jsonwebtoken');
const bcrypt = require('bcryptjs');
const crypto = require(""crypto"");
const config = require(""../../config"");
const uuid4 = require('uuid4');
const { ReturnValue } = require(""../ReturnValue"");
const { ReturnValues } = require(""../ReturnValues"");

function generateJwtToken(manager) {
    let data = { id : manager.id, managerid : manager.ManagerID };
    let expired = { expiresIn: '150m' };
    let token = jwt.sign(data, config.secret, expired);
    return token;
}

function reverseJwtToken(token) {
    return jwt.decode(token, config.secret);
};

async function hashAsync(password) {
    return await bcrypt.hash(password, 10);
}

async function compareAsync(password, passwordHash) {
    return bcrypt.compare(password, passwordHash);
}

function hash(password, callback) {
    bcrypt.hash(password, 10, function(err, hashresult) {
        callback(hashresult);
    });
}

function randomTokenString() {
    return crypto.randomBytes(40).toString('hex');
}

function tokenGet(header) {
    let token = """";

    try {
        if (header !== null && header !== undefined) {
            let tmp = String(header).trim();

            if (tmp !== null && tmp !== undefined && tmp !== """") {
                if (tmp === ""eyemcast-test"") {
                    token = { id : 1 };
                } else {
                    token = reverseJwtToken(tmp);
                }
                
            }
        }
    } catch (e) {
        console.log(""tokenGet Error : "", e.message);
    }

    return token;
}

const uuid = () => {
    const tokens = uuid4().split('-');
    return tokens[2] + tokens[1] + tokens[0] + tokens[3] + tokens[4];
}

module.exports = {
    generateJwtToken,
    hash,
    hashAsync,
    randomTokenString,
    uuid,
    compareAsync,
    reverseJwtToken,
    tokenGet
};
                    ";
        }

        public string NodeMailHelperCreate(BindOption options)
        {
            return @"
const { sendEmailSync } = require(""../../models/sendmail"");
const config = require('../../config');
const { ReturnValue } = require(""../ReturnValue"");

const mailHelper = {
    Send : (email, title, body) => {
        var result = new ReturnValue();

        try {
            sendEmailSync({
                to: email,
                subject: title,
                html: body
            });

            result.Success(1, email);
        } catch (e) {
            result.Error(e.message);
        }

        return result;
    }
};

module.exports = mailHelper;
";
        }

        public string NodeSequelizeDbCreate(BindOption options)
        {
            StringBuilder builder = new StringBuilder(200);
            builder.AppendLine("const config = require('../config');");
            builder.AppendLine("const { Sequelize } = require('sequelize');");
            builder.AppendLine("const { hashAsync } = require(\"./helpers/cryptoHelper\");");
            builder.AppendLine("");
            builder.AppendLine("module.exports = db = {};");
            builder.AppendLine("");
            builder.AppendLine("initialize();");
            builder.AppendLine("");
            builder.AppendLine("async function initialize() {");
            builder.AppendTabLine(1, "const { host, port, user, password, database } = config.database;");
            builder.AppendLine("");
            builder.AppendTabLine(1, "var seqConfig = {");
            builder.AppendTabLine(2, "host: host, ");
            builder.AppendTabLine(2, "dialect: 'mssql',");
            builder.AppendTabLine(2, "timezone: '+09:00',");
            builder.AppendTabLine(2, "dialectOptions: {");
            builder.AppendTabLine(3, "charset: 'utf8mb4',");
            builder.AppendTabLine(3, "dateStrings: true,");
            builder.AppendTabLine(3, "typeCast: true");
            builder.AppendTabLine(2, "},");
            builder.AppendTabLine(2, "define: {");
            builder.AppendTabLine(3, "timestamps: true");
            builder.AppendTabLine(2, "}");
            builder.AppendTabLine(1, "};");
            builder.AppendLine("");
            builder.AppendTabLine(1, "const sequelize = new Sequelize(database, user, password, seqConfig);");
            builder.AppendTabLine(1, "db.origin = sequelize;");
            foreach(var entity in options.tables.Where(x => x.ObjectType.Equals("TABLE", StringComparison.OrdinalIgnoreCase)))
            {
                builder.AppendTabLine(1, $"db.{entity.name} = require('./entities/{entity.name.FirstCharToLower()}')(sequelize);");
            }
            builder.AppendLine("");
            foreach (var entity in options.tables.Where(x => x.ObjectType.Equals("TABLE", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var fk in options.GetParentForeignKeys(entity.name))
                {
                    builder.AppendTab(1, $"db.{fk.ParentTableName}.hasMany(db.{fk.ReferencedTableName},");
                    builder.Append(" { ");
                    builder.Append($"foreignKey: '{fk.ReferencedColumnName}', sourceKey:'{fk.ParentColumnName}', onDelete:'CASCADE'");
                    builder.AppendLine(" });");

                    builder.AppendTab(1, $"db.{fk.ReferencedTableName}.belongsTo(db.{fk.ParentTableName},");
                    builder.Append(" { ");
                    builder.Append($"foreignKey: '{fk.ReferencedColumnName}', sourceKey:'{fk.ParentColumnName}'");
                    builder.AppendLine(" });");
                    builder.AppendLine("");
                }
            }
            builder.AppendTabLine(1, "await sequelize.sync({ force: false })");
            builder.AppendTabLine(1, ".catch(e => {");
            builder.AppendTabLine(2, "console.log('error:', e);");
            builder.AppendTabLine(1, "})");
            builder.AppendTabLine(1, ".then(() => {");
            builder.AppendTabLine(2, "console.log('sequelize OK');");
            builder.AppendTabLine(1, "});");
            builder.AppendLine("}");
            return builder.ToString();
        }

        public string NodeRouteIndexCreate(BindOption option, DbEntity entity)
        {
            StringBuilder builder = new StringBuilder(200);

            builder.AppendLine("const Router = require(\"express\").Router();");
            builder.AppendLine($"const Controller = require(\"./{entity.name.FirstCharToLower()}.controller\");");
            builder.AppendLine("const { ReturnValue, ReturnValues } = require(\"../../models/ReturnValue\");");
            builder.AppendLine("const cryptoHelper = require(\"../../models/helpers/cryptoHelper\");");
            builder.AppendLine("const config = require(\"../../config\");");
            builder.AppendEmptyLine();
            builder.AppendLine("Router.get(\"/list\", async (req, res) => {");
            builder.AppendTabLine(1, "let result = new ReturnValues();");
            builder.AppendTabLine(1, "let header_token = req.header(config.tokenKey);");
            builder.AppendTabLine(1, "let token = cryptoHelper.tokenGet(header_token);");
            builder.AppendTabLine(1, $"");
            builder.AppendTabLine(1, "if (token !== null && token !== undefined && token.id !== null && token.id !== undefined) {");
            builder.AppendTabLine(2, "try {");
            builder.AppendTabLine(3, $"result = await Controller.List(token.id, req.query);");
            builder.AppendTabLine(2, "} catch (e) {");
            builder.AppendTabLine(3, "result.Error(e.message);");
            builder.AppendTabLine(2, "}");
            builder.AppendTabLine(1, "} else {");
            builder.AppendTabLine(2, "result.Error(\"Authorization header not found\");");
            builder.AppendTabLine(1, "}");
            builder.AppendEmptyLine();
            builder.AppendTabLine(1, $"res.status(200).json(result);");
            builder.AppendLine("});");
            builder.AppendEmptyLine();
            builder.AppendLine("Router.get(\"/view/:id\", async (req, res) => {");
            builder.AppendTabLine(1, "let result = new ReturnValues();");
            builder.AppendTabLine(1, "let header_token = req.header(config.tokenKey);");
            builder.AppendTabLine(1, "let token = cryptoHelper.tokenGet(header_token);");
            builder.AppendTabLine(1, $"");
            builder.AppendTabLine(1, "if (token !== null && token !== undefined && token.id !== null && token.id !== undefined) {");
            builder.AppendTabLine(2, "try {");
            builder.AppendTabLine(3, "if (req.params.id !== null && req.params.id !== undefined) {");
            builder.AppendTabLine(4, "result = await Controller.Detail(token.id, req.params.id);");
            builder.AppendTabLine(3, "} else {");
            builder.AppendTabLine(4, "result.Error('NotFound Target');");
            builder.AppendTabLine(3, "}");
            builder.AppendTabLine(2, "} catch (e) {");
            builder.AppendTabLine(3, "result.Error(e.message);");
            builder.AppendTabLine(2, "}");
            builder.AppendTabLine(1, "} else {");
            builder.AppendTabLine(2, "result.Error(\"Authorization header not found\");");
            builder.AppendTabLine(1, "}");
            builder.AppendEmptyLine();
            builder.AppendTabLine(1, $"res.status(200).json(result);");
            builder.AppendLine("});");
            builder.AppendEmptyLine();
            builder.AppendLine("Router.post(\"/save\", async (req, res) => {");
            builder.AppendTabLine(1, "let result = new ReturnValues();");
            builder.AppendTabLine(1, "let header_token = req.header(config.tokenKey);");
            builder.AppendTabLine(1, "let token = cryptoHelper.tokenGet(header_token);");
            builder.AppendTabLine(1, $"");
            builder.AppendTabLine(1, "if (token !== null && token !== undefined && token.id !== null && token.id !== undefined) {");
            builder.AppendTabLine(2, "try {");
            builder.AppendTabLine(3, "if (req.body !== null && req.body !== undefined) {");
            builder.AppendTabLine(4, "result = await Controller.Save(token.id, req.body);");
            builder.AppendTabLine(3, "} else {");
            builder.AppendTabLine(4, "result.Error('Required input');");
            builder.AppendTabLine(3, "}");
            builder.AppendTabLine(2, "} catch (e) {");
            builder.AppendTabLine(3, "result.Error(e.message);");
            builder.AppendTabLine(2, "}");
            builder.AppendTabLine(1, "} else {");
            builder.AppendTabLine(2, "result.Error(\"Authorization header not found\");");
            builder.AppendTabLine(1, "}");
            builder.AppendEmptyLine();
            builder.AppendTabLine(1, $"res.status(200).json(result);");
            builder.AppendLine("});");
            builder.AppendLine($"");
            builder.AppendEmptyLine();
            builder.AppendLine("Router.delete(\"/erase/:id\", async (req, res) => {");
            builder.AppendTabLine(1, "let result = new ReturnValues();");
            builder.AppendTabLine(1, "let header_token = req.header(config.tokenKey);");
            builder.AppendTabLine(1, "let token = cryptoHelper.tokenGet(header_token);");
            builder.AppendTabLine(1, $"");
            builder.AppendTabLine(1, "if (token !== null && token !== undefined && token.id !== null && token.id !== undefined) {");
            builder.AppendTabLine(2, "try {");
            builder.AppendTabLine(3, "if (req.params.id !== null && req.params.id !== undefined) {");
            builder.AppendTabLine(4, "result = await Controller.Remove(token.id, req.params.id);");
            builder.AppendTabLine(3, "} else {");
            builder.AppendTabLine(4, "result.Error('Required target');");
            builder.AppendTabLine(3, "}");
            builder.AppendTabLine(2, "} catch (e) {");
            builder.AppendTabLine(3, "result.Error(e.message);");
            builder.AppendTabLine(2, "}");
            builder.AppendTabLine(1, "} else {");
            builder.AppendTabLine(2, "result.Error(\"Authorization header not found\");");
            builder.AppendTabLine(1, "}");
            builder.AppendEmptyLine();
            builder.AppendTabLine(1, $"res.status(200).json(result);");
            builder.AppendLine("});");
            builder.AppendEmptyLine();
            builder.AppendLine($"module.exports = Router;");
            
            return builder.ToString();
        }

        public string NodeRouteControllerCreate(BindOption option, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryColumn = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryColumn == null)
                {
                    primaryColumn = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                string entityName = info[0].TableName;

                builder.AppendLine("const db = require('../../models/db');");
                builder.AppendLine("const { ReturnValue, ReturnValues } = require(\"../../models/ReturnValue\");");
                builder.AppendEmptyLine();
                builder.AppendLine("exports.List = async (userid, query) => {");
                builder.AppendTabLine(1, "let result = new ReturnValues();");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "try {");
                builder.AppendTab(2, $"let list = await db.{entityName}.findAll(");
                builder.AppendLine("{");
                builder.AppendTabLine(3, "//검색조건 추가");
                builder.AppendTabLine(2, "});");
                builder.AppendEmptyLine();
                builder.AppendTabLine(2, "if (list === null || list === undefined) {");
                builder.AppendTabLine(3, "result.check = true;");
                builder.AppendTabLine(3, "result.code = 0;");
                builder.AppendTabLine(2, "} else {");
                builder.AppendTabLine(3, "result.Success(list.length, list, '', '');");
                builder.AppendTabLine(2, "}");
                builder.AppendTabLine(1, "} catch (e) {");
                builder.AppendTabLine(2, "result.Error(e.message);");
                builder.AppendTabLine(1, "} finally {");
                builder.AppendTabLine(2, "return result;");
                builder.AppendTabLine(1, "}");
                builder.AppendLine("};");
                builder.AppendEmptyLine();
                builder.AppendLine("exports.Detail = async (userid, targetid) => {");
                builder.AppendTabLine(1, "let result = new ReturnValues();");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "try {");
                builder.AppendTab(2, $"let detail = await db.{entityName}.findOne(");
                builder.AppendLine("{");
                builder.AppendTabLine(2, "});");
                builder.AppendEmptyLine();
                builder.AppendTabLine(2, "if (detail !== null && detail !== undefined) {");
                if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
                {
                    builder.AppendTabLine(3, $"result.Success(detail.{primaryColumn.ColumnName}, detail, '', '');");
                }
                else
                {
                    builder.AppendTabLine(3, $"result.Success(targetid, detail, '', '');");
                }
                builder.AppendTabLine(2, "} else {");
                builder.AppendTabLine(3, "result.Error('NotFound Target');");
                builder.AppendTabLine(2, "}");
                builder.AppendTabLine(1, "} catch (e) {");
                builder.AppendTabLine(2, "result.Error(e.message);");
                builder.AppendTabLine(1, "} finally {");
                builder.AppendTabLine(2, "return result;");
                builder.AppendTabLine(1, "}");
                builder.AppendLine("};");

                builder.AppendEmptyLine();
                builder.AppendLine("exports.Remove = async (userid, id) => {");
                builder.AppendTabLine(1, "let result = new ReturnValues();");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "try {");
                builder.AppendTab(2, $"let detail = await db.{entityName}.findOne(");
                builder.AppendLine("{");
                if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
                {
                    builder.AppendTabLine(3, "where : { " + primaryColumn.ColumnName + " : id }");
                }
                builder.AppendTabLine(2, "});");
                builder.AppendTabLine(2, "");
                builder.AppendTabLine(2, "if (detail !== null && detail !== undefined) {");
                builder.AppendTab(3, $"await db.{entityName}.destroy(");
                builder.AppendLine("{");
                if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
                {
                    builder.AppendTabLine(4, "where : { " + primaryColumn.ColumnName + " : id }");
                }
                builder.AppendTabLine(3, "});");
                builder.AppendTabLine(2, "");
                builder.AppendTabLine(3, "result.Success(id, '', '');");
                builder.AppendTabLine(2, "} else {");
                builder.AppendTabLine(3, "result.Error(\"NotFound Target\");");
                builder.AppendTabLine(2, "}");
                builder.AppendTabLine(1, "} catch (e) {");
                builder.AppendTabLine(2, "result.Error(e.message);");
                builder.AppendTabLine(1, "} finally {");
                builder.AppendTabLine(2, "return result;");
                builder.AppendTabLine(1, "}");
                builder.AppendLine("};");

                builder.AppendEmptyLine();
                builder.AppendLine("exports.Save = async (userid, body) => {");
                builder.AppendTabLine(1, "let result = new ReturnValues();");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "try {");
                builder.AppendTab(2, $"let detail = await db.{entityName}.findOne(");
                builder.AppendLine("{");
                if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
                {
                    builder.AppendTabLine(3, "where : { " + primaryColumn.ColumnName + " : id }");
                }
                builder.AppendTabLine(2, "});");
                builder.AppendEmptyLine();
                builder.AppendTabLine(2, "if (detail !== null && detail !== undefined) {");
                builder.AppendTabLine(3, "//update");
                foreach (var item in option.GetTableProperties(entityName))
                {
                    builder.AppendTabLine(3, $"detail.{item.Name} = body.{item.Name.FirstCharToLower()};");
                }
                builder.AppendTabLine(3, "await detail.save();");
                if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
                {
                    builder.AppendEmptyLine();
                    builder.AppendTabLine(3, $"if (detail.{primaryColumn.ColumnName} > 0) " + "{");
                    builder.AppendTabLine(4, $"result.Success(detail.{primaryColumn.ColumnName}, \"\", \"\");");
                    builder.AppendTabLine(3, "} else {");
                    builder.AppendTabLine(4, "result.Error(\"Save Fail\");");
                    builder.AppendTabLine(3, "}");
                }
                builder.AppendTabLine(2, "} else {");
                builder.AppendTabLine(3, "//insert");
                builder.AppendTabLine(3, $"let target = new db.{entityName}();");
                foreach (var item in info)
                {
                    builder.AppendTabLine(3, $"target.{item.Name} = body.{item.Name.FirstCharToLower()};");
                }
                builder.AppendTabLine(3, "await target.save();");
                if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
                {
                    builder.AppendEmptyLine();
                    builder.AppendTabLine(3, $"if (target.{primaryColumn.ColumnName} > 0) " + "{");
                    builder.AppendTabLine(4, $"result.Success(target.{primaryColumn.ColumnName}, \"\", \"\");");
                    builder.AppendTabLine(3, "} else {");
                    builder.AppendTabLine(4, "result.Error(\"Save Fail\");");
                    builder.AppendTabLine(3, "}");
                }
                builder.AppendTabLine(2, "}");
                builder.AppendTabLine(1, "} catch (e) {");
                builder.AppendTabLine(2, "result.Error(e.message);");
                builder.AppendTabLine(1, "} finally {");
                builder.AppendTabLine(2, "return result;");
                builder.AppendTabLine(1, "}");
                builder.AppendLine("};");
            }

            return builder.ToString();
        }


        public string NodeRouteControllerCreate(BindOption option, DbEntity entity)
        {
            StringBuilder builder = new StringBuilder(200);

            builder.AppendLine("const db = require('../../models/db');");
            builder.AppendLine("const { ReturnValue, ReturnValues } = require(\"../../models/ReturnValue\");");
            builder.AppendEmptyLine();
            builder.AppendLine("exports.List = async (userid, query) => {");
            builder.AppendTabLine(1, "let result = new ReturnValues();");
            builder.AppendEmptyLine();
            builder.AppendTabLine(1, "try {");
            builder.AppendTab(2, $"let list = await db.{entity.name}.findAll(");
            builder.AppendLine("{");
            builder.AppendTabLine(3, "//검색조건 추가");
            builder.AppendTabLine(2, "});");
            builder.AppendEmptyLine();
            builder.AppendTabLine(2, "if (list === null || list === undefined) {");
            builder.AppendTabLine(3, "result.check = true;");
            builder.AppendTabLine(3, "result.code = 0;");
            builder.AppendTabLine(2, "} else {");
            builder.AppendTabLine(3, "result.Success(list.length, list, '', '');");
            builder.AppendTabLine(2, "}");
            builder.AppendTabLine(1, "} catch (e) {");
            builder.AppendTabLine(2, "result.Error(e.message);");
            builder.AppendTabLine(1, "} finally {");
            builder.AppendTabLine(2, "return result;");
            builder.AppendTabLine(1, "}");
            builder.AppendLine("};");
            builder.AppendEmptyLine();
            builder.AppendLine("exports.Detail = async (userid, targetid) => {");
            builder.AppendTabLine(1, "let result = new ReturnValues();");
            builder.AppendEmptyLine();
            builder.AppendTabLine(1, "try {");
            builder.AppendTab(2, $"let detail = await db.{entity.name}.findOne(");
            builder.AppendLine("{");
            var primaryColumn = option.GetTableProperties(entity.name).Where(x => x.is_identity).FirstOrDefault();
            if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
            {
                builder.AppendTabLine(3, "where : { " + primaryColumn.ColumnName + " : targetid }");
            }
            builder.AppendTabLine(2, "});");
            builder.AppendEmptyLine();
            builder.AppendTabLine(2, "if (detail !== null && detail !== undefined) {");
            if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
            {
                builder.AppendTabLine(3, $"result.Success(detail.{primaryColumn.ColumnName}, detail, '', '');");
            }
            else
            {
                builder.AppendTabLine(3, $"result.Success(targetid, detail, '', '');");
            }
            builder.AppendTabLine(2, "} else {");
            builder.AppendTabLine(3, "result.Error('NotFound Target');");
            builder.AppendTabLine(2, "}");
            builder.AppendTabLine(1, "} catch (e) {");
            builder.AppendTabLine(2, "result.Error(e.message);");
            builder.AppendTabLine(1, "} finally {");
            builder.AppendTabLine(2, "return result;");
            builder.AppendTabLine(1, "}");
            builder.AppendLine("};");

            builder.AppendEmptyLine();
            builder.AppendLine("exports.Remove = async (userid, id) => {");
            builder.AppendTabLine(1, "let result = new ReturnValues();");
            builder.AppendEmptyLine();
            builder.AppendTabLine(1, "try {");
            builder.AppendTab(2, $"let detail = await db.{entity.name}.findOne(");
            builder.AppendLine("{");
            if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
            {
                builder.AppendTabLine(3, "where : { " + primaryColumn.ColumnName + " : id }");
            }
            builder.AppendTabLine(2, "});");
            builder.AppendTabLine(2, "");
            builder.AppendTabLine(2, "if (detail !== null && detail !== undefined) {");
            builder.AppendTab(3, $"await db.{entity.name}.destroy(");
            builder.AppendLine("{");
            if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
            {
                builder.AppendTabLine(4, "where : { " + primaryColumn.ColumnName + " : id }");
            }
            builder.AppendTabLine(3, "});");
            builder.AppendTabLine(2, "");
            builder.AppendTabLine(3, "result.Success(id, '', '');");
            builder.AppendTabLine(2, "} else {");
            builder.AppendTabLine(3, "result.Error(\"NotFound Target\");");
            builder.AppendTabLine(2, "}");
            builder.AppendTabLine(1, "} catch (e) {");
            builder.AppendTabLine(2, "result.Error(e.message);");
            builder.AppendTabLine(1, "} finally {");
            builder.AppendTabLine(2, "return result;");
            builder.AppendTabLine(1, "}");
            builder.AppendLine("};");

            builder.AppendEmptyLine();
            builder.AppendLine("exports.Save = async (userid, body) => {");
            builder.AppendTabLine(1, "let result = new ReturnValues();");
            builder.AppendEmptyLine();
            builder.AppendTabLine(1, "try {");
            builder.AppendTab(2, $"let detail = await db.{entity.name}.findOne(");
            builder.AppendLine("{");
            if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
            {
                builder.AppendTabLine(3, "where : { " + primaryColumn.ColumnName + " : id }");
            }
            builder.AppendTabLine(2, "});");
            builder.AppendEmptyLine();
            builder.AppendTabLine(2, "if (detail !== null && detail !== undefined) {");
            builder.AppendTabLine(3, "//update");
            foreach(var item in option.GetTableProperties(entity.name))
            {
                builder.AppendTabLine(3, $"detail.{item.Name} = body.{item.Name.FirstCharToLower()};");
            }
            builder.AppendTabLine(3, "await detail.save();");
            if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
            {
                builder.AppendEmptyLine();
                builder.AppendTabLine(3, $"if (detail.{primaryColumn.ColumnName} > 0) " + "{");
                builder.AppendTabLine(4, $"result.Success(detail.{primaryColumn.ColumnName}, \"\", \"\");");
                builder.AppendTabLine(3, "} else {");
                builder.AppendTabLine(4, "result.Error(\"Save Fail\");");
                builder.AppendTabLine(3, "}");
            }
            builder.AppendTabLine(2, "} else {");
            builder.AppendTabLine(3, "//insert");
            builder.AppendTabLine(3, $"let target = new db.{entity.name}();");
            foreach (var item in option.GetTableProperties(entity.name))
            {
                builder.AppendTabLine(3, $"target.{item.Name} = body.{item.Name.FirstCharToLower()};");
            }
            builder.AppendTabLine(3, "await target.save();");
            if (primaryColumn != null && !string.IsNullOrWhiteSpace(primaryColumn.ColumnName))
            {
                builder.AppendEmptyLine();
                builder.AppendTabLine(3, $"if (target.{primaryColumn.ColumnName} > 0) " + "{");
                builder.AppendTabLine(4, $"result.Success(target.{primaryColumn.ColumnName}, \"\", \"\");");
                builder.AppendTabLine(3, "} else {");
                builder.AppendTabLine(4, "result.Error(\"Save Fail\");");
                builder.AppendTabLine(3, "}");
            }
            builder.AppendTabLine(2, "}");
            builder.AppendTabLine(1, "} catch (e) {");
            builder.AppendTabLine(2, "result.Error(e.message);");
            builder.AppendTabLine(1, "} finally {");
            builder.AppendTabLine(2, "return result;");
            builder.AppendTabLine(1, "}");
            builder.AppendLine("};");

            return builder.ToString();
        }

        public string NodeRootRouteIndexCreate(BindOption option)
        {
            StringBuilder builder = new StringBuilder(200);

            builder.AppendLine("const router = require(\"express\").Router();");

            foreach (var entity in option.tables)
            {
                builder.AppendLine($"const {entity.name.FirstCharToLower()} = require(\"./{entity.name.FirstCharToLower()}\");");
            }
            builder.AppendEmptyLine();
            foreach (var entity in option.tables)
            {
                builder.AppendLine($"router.use(\"/{entity.name.FirstCharToLower()}\", {entity.name.FirstCharToLower()});");
            }
            builder.AppendEmptyLine();
            builder.AppendLine("module.exports = router;");

            return builder.ToString();
        }

        public string NodeReturnValue(BindOption option)
        {
            return @"class ReturnValue {
    check = false;
    code = -1;
    value = """";
    message = """";

    constructor() {
    }

    Success = (code, value, msg) => {
        this.check = true;
        this.code = code;
        this.value = value;
        this.message = msg;
    };

    Error = (msg) => {
        this.check = false;
        this.code = -1;
        this.message = msg;
    }
};

module.exports = {
    ReturnValue
};";
        }

        public string NodeReturnValues(BindOption option)
        {
            return @"class ReturnValues {
    check = false;
    code = -1;
    value = """";
    message = """";
    data = null;

    constructor() {
    }

    Success = (code, data, value, msg) => {
        this.check = true;
        this.code = code;
        this.data = data;
        this.value = value;
        this.message = msg;
    };

    Error = (msg) => {
        this.check = false;
        this.code = -1;
        this.message = msg;
    }
};

module.exports = {
    ReturnValues
};";
        }

        public string NodeLogger(BindOption option)
        {
            return @"const fs = require('fs');
const moment = require('moment');

const LOG_DIR = './Log/';
const LOG_LEVEL = {DEBUG: 1, INFO: 2, WARN: 3, ERROR: 4, FATAL: 5};
const MAX_SIZE = 1024 * 1024 * 4;

class Logger {
  constructor() {
    if (!fs.existsSync(LOG_DIR)) {
      fs.mkdirSync(LOG_DIR);
    }
  }

  _writeLog(level, message) {
    const date = moment().format('YYMMDD');
    const time = moment().format('HHmmss');

    if (!fs.existsSync(`${LOG_DIR}/${date}`)) {
        fs.mkdirSync(`${LOG_DIR}/${date}`);
    }

    const logFile = `${LOG_DIR}/${date}/${moment().format('DDHH')}.log`;

    const logMessage = `[${moment().format('YYYY-MM-DD HH:mm:ss')}] [${level}] ${message}\n`;

    fs.stat(logFile, (err, stats) => {
      if (err) {
        fs.appendFileSync(logFile, logMessage);
      } else {
        if (stats.size >= MAX_SIZE) {
          const backupFile = `${LOG_DIR}/${date}_${time}.log`;

          fs.renameSync(logFile, backupFile);
          fs.appendFileSync(logFile, logMessage);
        } else {
          fs.appendFileSync(logFile, logMessage);
        }
      }
    });
  }

  debug(message) {
    this._writeLog('DEBUG', message);
  }

  info(message) {
    this._writeLog('INFO', message);
  }

  warn(message) {
    this._writeLog('WARN', message);
  }

  error(message) {
    this._writeLog('ERROR', message);
  }

  fatal(message) {
    this._writeLog('FATAL', message);
  }
}

module.exports = new Logger();";
        }

        public string NodePusher(BindOption option)
        {
            return @"const admin = require('firebase-admin');
const serviceAccount = require('../key/service-account.json');
const config = require(""../config"");

class Pusher {
  constructor() {
    admin.initializeApp({
        credential: admin.credential.cert(serviceAccount)
    });
  }

  async _sendproc(token, title, body) {
    const message = {
        ""token"": token,
        ""notification"": {
          ""body"": body,
          ""title"": title,
        },
        ""android"": {
          ""notification"": {
            ""channel_id"": config.fcm.channel_id,
            ""sound"": (config.fcm.sound !== null && config.fcm.sound !== undefined && String(config.fcm.sound).trim() !== """") ? config.fcm.sound : """"
          }
        }
    };

    admin.messaging().send(message)
    .then((response) => {
      console.log(""Successfully sent message:"", response);
    })
    .catch((error) => {
      console.log(""Error sending message:"", error);
    });
  }

  async send(token, title, body) {
    await this._sendproc(token, title, body);
  }

  sendSync(token, title, body) {
    this._sendproc(token, title, body);
  }
}

module.exports = new Pusher();";
        }
    }
}
