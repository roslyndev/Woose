using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

                if (option.IsUseApiOperation)
                {
                    builder.AppendLine("import { ApiProperty } from '@nestjs/swagger';");
                }
                builder.AppendLine("import { " + entityName + " } from 'src/entities';");
                builder.AppendEmptyLine();
                builder.AppendLine("class " + entityName.FirstCharToUpper() + "Regist {");
                foreach (var item in info)
                {
                    if (!item.is_identity && !item.IsDate)
                    {
                        if (option.IsUseApiOperation)
                        {
                            builder.AppendTabLine(1, "@ApiProperty({ description: '" + item.Name + "' })");
                        }
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
                        if (option.IsUseApiOperation)
                        {
                            builder.AppendTabLine(1, "@ApiProperty({ description: '" + item.Name + "' })");
                        }
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
                bool isKey = false;

                if (option.IsUseApiOperation)
                {
                    builder.AppendLine("import { ApiProperty } from '@nestjs/swagger';");
                }
                builder.AppendLine("import { Entity,Column,PrimaryGeneratedColumn,CreateDateColumn,UpdateDateColumn,PrimaryColumn,ManyToOne,JoinColumn } from 'typeorm';");
                var fks = option.GetParentForeignKeys(entityName);
                if (fks != null && fks.Count > 0)
                {
                    foreach(var fk in fks)
                    {
                        builder.AppendLine("import { " + fk.ReferencedTableName + " } from './" + fk.ReferencedTableName.ToLower() + ".entity';");
                    }

                    isKey = (fks.Count == info.Count);
                    if (!isKey)
                    {
                        isKey = (info.Where(x => x.is_identity).Count() < 1);
                    }
                }

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
                        if (fks.Where(x => x.ParentColumnName == item.ColumnName).Any())
                        {
                            var fk = fks.Where(x => x.ParentColumnName == item.ColumnName).First();
                            if (option.IsUseApiOperation)
                            {
                                builder.AppendTabLine(1, "@ApiProperty({ description: '" + item.Name + "' })");
                            }
                            builder.AppendTabLine(1, "@ManyToOne(() => " + fk.ReferencedTableName + ")");
                            builder.AppendTabLine(1, "@JoinColumn({ name: '" + fk.ReferencedColumnName + "' })");
                            if (isKey)
                            {
                                builder.AppendTabLine(1, "@PrimaryColumn({ type: '" + item.ColumnType + "'})");
                            }
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
                                    if (option.IsUseApiOperation)
                                    {
                                        builder.AppendTabLine(1, "@ApiProperty({ description: '" + item.Name + "' })");
                                    }
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
                                if (option.IsUseApiOperation)
                                {
                                    builder.AppendTabLine(1, "@ApiProperty({ description: '" + item.Name + "' })");
                                }
                                builder.AppendTab(1, "@Column('" + item.ColumnType + "', { name: '" + item.Name + "'");
                                if (item.IsSize)
                                {
                                    builder.Append(", length : " + Convert.ToString(item.max_length));
                                }
                                builder.AppendLine(" })");
                            }
                        }
                    }
                    builder.AppendTabLine(1, $"{item.Name.FirstCharToLower()}: {item.ScriptType};");
                    builder.AppendEmptyLine();
                }
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        public string ControllerCreate(BindOption option, List<DbTableInfo> info)
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
                bool isKey = false;

                if (!option.UsingCustomModel)
                {
                    builder.Append("import { ApiTags,ApiBearerAuth");
                    if (option.IsUseApiOperation)
                    {
                        builder.Append(",ApiOperation");
                    }
                    builder.AppendLine(" } from '@nestjs/swagger';");
                    builder.AppendLine("import { Controller,Get,Post,Put,Body,Param,Delete,UseInterceptors,Req } from '@nestjs/common';");
                    builder.AppendLine("import { AuthInterceptor } from '../Interceptors';");
                    builder.AppendLine("import { " + entityName + "Service } from '../services';");
                    //builder.AppendLine("import { " + entityName + " } from '../entities';");
                    builder.AppendLine("import { ReturnValue, ReturnValues } from '../models';");
                    builder.AppendLine("import { " + entityName + "Regist, " + entityName + "Update } from '../dto';");
                    builder.AppendEmptyLine();
                    builder.AppendLine($"@ApiTags(\"{entityName.ToLower()}\")");
                    if (!option.IsNoModel)
                    {
                        builder.AppendLine("@ApiBearerAuth(\"AccessToken\")");
                        builder.AppendLine("@UseInterceptors(AuthInterceptor)");
                    }
                    builder.AppendLine($"@Controller('{entityName.ToLower()}')");
                    builder.AppendLine("export class " + entityName + "Controller {");
                    builder.AppendTabLine(1, "constructor(private readonly service: " + entityName + "Service) {}");
                    builder.AppendEmptyLine();
                }

                builder.AppendTabLine(1, "@Post(\"" + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "/regist\")");
                if (option.IsUseApiOperation)
                {
                    builder.AppendTabLine(1, "@ApiOperation({ summary: '" + entityName + " Regist', description: '" + entityName + " Regist Api Method' })");
                }
                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "create(" + ((!option.IsNoModel) ? "@Req() request, " : "") + "@Body() data:" + entityName + "Regist): Promise<ReturnValue> {");
                builder.AppendTabLine(2, "return await this.service." + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "create(" + ((!option.IsNoModel) ? " request.accessToken, " : "") + "data);");
                builder.AppendTabLine(1, "}");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "@Put(\"" + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "/update\")");
                if (option.IsUseApiOperation)
                {
                    builder.AppendTabLine(1, "@ApiOperation({ summary: '" + entityName + " Update', description: '" + entityName + " Modify Api Method' })");
                }
                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "update(" + ((!option.IsNoModel) ? "@Req() request, " : "") + "@Body() data:" + entityName + "Update): Promise<ReturnValue> {");
                builder.AppendTabLine(2, "return await this.service." + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "update(" + ((!option.IsNoModel) ? " request.accessToken, " : "") + "data);");
                builder.AppendTabLine(1, "}");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "@Get(\"" + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "/list\")");
                if (option.IsUseApiOperation)
                {
                    builder.AppendTabLine(1, "@ApiOperation({ summary: '" + entityName + " List', description: '" + entityName + " List Api Method' })");
                }
                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "findlist(" + ((!option.IsNoModel) ? "@Req() request, " : "") + "): Promise<ReturnValues> {");
                builder.AppendTabLine(2, "return await this.service." + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "findlist(" + ((!option.IsNoModel) ? " request.accessToken " : "") + ");");
                builder.AppendTabLine(1, "}");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "@Get('" + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "/view/:id')");
                if (option.IsUseApiOperation)
                {
                    builder.AppendTabLine(1, "@ApiOperation({ summary: '" + entityName + " Detail', description: '" + entityName + " Detail View Api Method' })");
                }
                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "findById(" + ((!option.IsNoModel) ? "@Req() request, " : "") + "@Param('id') id: number): Promise<ReturnValues> {");
                builder.AppendTabLine(2, "return await this.service." + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "findById(" + ((!option.IsNoModel) ? " request.accessToken, " : "") + "id);");
                builder.AppendTabLine(1, "}");
                builder.AppendEmptyLine();
                builder.AppendTabLine(1, "@Delete('" + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "/erase/:id')");
                if (option.IsUseApiOperation)
                {
                    builder.AppendTabLine(1, "@ApiOperation({ summary: '" + entityName + " Erase', description: '" + entityName + " Erase Api Method' })");
                }
                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "eraseById(" + ((!option.IsNoModel) ? "@Req() request, " : "") + "@Param('id') id: number): Promise<ReturnValues> {");
                builder.AppendTabLine(2, "return await this.service." + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "eraseById(" + ((!option.IsNoModel) ? " request.accessToken, " : "") + "id);");
                builder.AppendTabLine(1, "}");

                if (!option.UsingCustomModel)
                {
                    builder.AppendLine("}");
                }
            }

            return builder.ToString();
        }

        public string ServiceCreate(BindOption option, List<DbTableInfo> info)
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

                if (!option.UsingCustomModel)
                {
                    builder.AppendLine("import { Injectable, UnauthorizedException, } from '@nestjs/common';");
                    builder.AppendLine("import { InjectRepository } from '@nestjs/typeorm';");
                    builder.AppendLine("import { Repository } from 'typeorm';");
                    builder.AppendLine("import { " + entityName + " } from '../entities';");
                    builder.AppendLine("import { ReturnValue, ReturnValues } from '../models';");
                    builder.AppendLine("import { " + entityName + "Regist, " + entityName + "Update } from '../dto';");
                    builder.AppendLine("import { CryptoService } from './crypto.service';");
                    builder.AppendEmptyLine();
                    builder.AppendLine($"@Injectable()");
                    builder.AppendLine("export class " + entityName + "Service {");
                    builder.AppendTabLine(1, "constructor(");
                    builder.AppendTabLine(1, "@InjectRepository(" + entityName + ")");
                    builder.AppendTabLine(1, "private readonly repository: Repository<" + entityName + ">,");
                    builder.AppendTabLine(1, "private crypto:CryptoService");
                    builder.AppendTabLine(1, ") {}");
                    builder.AppendEmptyLine();
                }
                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "create(" + ((option.IsNoModel) ? "" : "accessToken:string, ") + "data:" + entityName + "Regist): Promise<ReturnValue> {");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(2, "if (accessToken !== null && accessToken !== undefined && accessToken !== '') {");
                    builder.AppendTabLine(3, "let auth = this.crypto.tokenGet(accessToken);");
                    builder.AppendTabLine(3, "if (auth !== null && auth !== undefined && auth.memberIDX > 0) {");
                }

                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "let result = new ReturnValue();");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), $"let {entityName.ToLower()}:{entityName} = new {entityName}();");
                foreach (var item in info)
                {
                    if (!item.is_identity)
                    {
                        if (item.IsDate)
                        {
                            builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), $"{entityName.ToLower()}.{item.ColumnName.FirstCharToLower()} = new Date();");
                        }
                        else
                        {
                            builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), $"{entityName.ToLower()}.{item.ColumnName.FirstCharToLower()} = data.{item.ColumnName.FirstCharToLower()};");
                        }
                    }
                }
                builder.AppendEmptyLine();
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), $"let target = await this.repository.save({entityName.ToLower()});");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "if (target !== null && target !== undefined) {");
                if (primaryKey != null)
                {
                    builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), $"result.Success({entityName.ToLower()}.{primaryKey.ColumnName.FirstCharToLower()}, \"\", \"\");");
                }
                else
                {
                    builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "result.Success(1, \"\", \"\");");
                }
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "} else {");
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "result.Error(\"Fail Save Data\");");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "}");
                builder.AppendEmptyLine();
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "return result;");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(3, "} else {");
                    builder.AppendTabLine(4, "throw new UnauthorizedException('Invalid access token');");
                    builder.AppendTabLine(3, "}");
                    builder.AppendTabLine(2, "} else {");
                    builder.AppendTabLine(3, "throw new UnauthorizedException('Required access token');");
                    builder.AppendTabLine(2, "}");
                }

                builder.AppendTabLine(1, "}");
                builder.AppendEmptyLine();

                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "update(" + ((option.IsNoModel) ? "" : "accessToken:string, ") + "data:" + entityName + "Update): Promise<ReturnValue> {");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(2, "if (accessToken !== null && accessToken !== undefined && accessToken !== '') {");
                    builder.AppendTabLine(3, "let auth = this.crypto.tokenGet(accessToken);");
                    builder.AppendTabLine(3, "if (auth !== null && auth !== undefined && auth.memberIDX > 0) {");
                }
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "let result = new ReturnValue();");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), $"let {entityName.ToLower()} = await this.repository.findOne(" + "{");
                if (primaryKey != null)
                {
                    builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "where: { " + primaryKey.ColumnName.FirstCharToLower() + ": data." + primaryKey.ColumnName.FirstCharToLower() + " }");
                }
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "});");
                builder.AppendEmptyLine();


                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), $"if ({entityName.ToLower()}) " + "{");
                foreach (var item in info)
                {
                    if (!item.is_identity)
                    {
                        if (item.IsDate)
                        {
                            builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), $"{entityName.ToLower()}.{item.ColumnName.FirstCharToLower()} = new Date();");
                        }
                        else
                        {
                            builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), $"{entityName.ToLower()}.{item.ColumnName.FirstCharToLower()} = data.{item.ColumnName.FirstCharToLower()};");
                        }
                    }
                }
                builder.AppendEmptyLine();
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), $"let target = await this.repository.save({entityName.ToLower()});");
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "if (target !== null && target !== undefined) {");
                if (primaryKey != null)
                {
                    builder.AppendTabLine(4 + ((!option.IsNoModel) ? 2 : 0), $"result.Success({entityName.ToLower()}.{primaryKey.ColumnName.FirstCharToLower()}, \"\", \"\");");
                }
                else
                {
                    builder.AppendTabLine(4 + ((!option.IsNoModel) ? 2 : 0), "result.Success(1, \"\", \"\");");
                }
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "} else {");
                builder.AppendTabLine(4 + ((!option.IsNoModel) ? 2 : 0), "result.Error(\"Fail Save Data\");");
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "}");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "} else {");
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "result.Error(\"NotFound Data\");");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "}");
                builder.AppendEmptyLine();
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "return result;");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(3, "} else {");
                    builder.AppendTabLine(4, "throw new UnauthorizedException('Invalid access token');");
                    builder.AppendTabLine(3, "}");
                    builder.AppendTabLine(2, "} else {");
                    builder.AppendTabLine(3, "throw new UnauthorizedException('Required access token');");
                    builder.AppendTabLine(2, "}");
                }

                builder.AppendTabLine(1, "}");
                builder.AppendEmptyLine();

                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "findlist(" + ((option.IsNoModel) ? "" : "accessToken:string, ") + "): Promise<ReturnValues> {");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(2, "if (accessToken !== null && accessToken !== undefined && accessToken !== '') {");
                    builder.AppendTabLine(3, "let auth = this.crypto.tokenGet(accessToken);");
                    builder.AppendTabLine(3, "if (auth !== null && auth !== undefined && auth.memberIDX > 0) {");
                }

                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "let result = new ReturnValues();");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "let data = await this.repository.find();");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "if (data !== null && data !== undefined && data.length > 0) {");
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "result.Success(data.length, data, \"\", \"\");");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "} else {");
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "result.Success(0, [], \"\", \"\");");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "}");
                builder.AppendEmptyLine();
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "return result;");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(3, "} else {");
                    builder.AppendTabLine(4, "throw new UnauthorizedException('Invalid access token');");
                    builder.AppendTabLine(3, "}");
                    builder.AppendTabLine(2, "} else {");
                    builder.AppendTabLine(3, "throw new UnauthorizedException('Required access token');");
                    builder.AppendTabLine(2, "}");
                }

                builder.AppendTabLine(1, "}");
                builder.AppendEmptyLine();

                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "findById(" + ((option.IsNoModel) ? "" : "accessToken:string, ") + "idx:number): Promise<ReturnValues> {");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(2, "if (accessToken !== null && accessToken !== undefined && accessToken !== '') {");
                    builder.AppendTabLine(3, "let auth = this.crypto.tokenGet(accessToken);");
                    builder.AppendTabLine(3, "if (auth !== null && auth !== undefined && auth.memberIDX > 0) {");
                }

                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "let result = new ReturnValues();");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "let data = await this.repository.findOne({");
                if (primaryKey != null)
                {
                    builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "where: { " + primaryKey.ColumnName.FirstCharToLower() + ": idx }");
                }
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "});");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "if (data !== null && data !== undefined) {");
                if (primaryKey != null)
                {
                    builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), $"result.Success(data.{primaryKey.ColumnName.FirstCharToLower()}, data, \"\", \"\");");
                }
                else
                {
                    builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "result.Success(idx, data, \"\", \"\");");
                }
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "} else {");
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "result.Error(\"NotFound data\");");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "}");
                builder.AppendEmptyLine();
                builder.AppendTabLine(2, "return result;");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(3, "} else {");
                    builder.AppendTabLine(4, "throw new UnauthorizedException('Invalid access token');");
                    builder.AppendTabLine(3, "}");
                    builder.AppendTabLine(2, "} else {");
                    builder.AppendTabLine(3, "throw new UnauthorizedException('Required access token');");
                    builder.AppendTabLine(2, "}");
                }

                builder.AppendTabLine(1, "}");
                builder.AppendEmptyLine();

                builder.AppendTabLine(1, "async " + ((option.UsingCustomModel) ? entityName.FirstCharToLower() : "") + "eraseById(" + ((option.IsNoModel) ? "" : "accessToken:string, ") + "idx:number): Promise<ReturnValues> {");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(2, "if (accessToken !== null && accessToken !== undefined && accessToken !== '') {");
                    builder.AppendTabLine(3, "let auth = this.crypto.tokenGet(accessToken);");
                    builder.AppendTabLine(3, "if (auth !== null && auth !== undefined && auth.memberIDX > 0) {");
                }

                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "let result = new ReturnValues();");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "let data = await this.repository.delete(idx)");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "if (data !== null && data !== undefined) {");
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "result.Success(idx, data, \"\", \"\");");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "} else {");
                builder.AppendTabLine(3 + ((!option.IsNoModel) ? 2 : 0), "result.Error(\"NotFound data\");");
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "}");
                builder.AppendEmptyLine();
                builder.AppendTabLine(2 + ((!option.IsNoModel) ? 2 : 0), "return result;");

                if (!option.IsNoModel)
                {
                    builder.AppendTabLine(3, "} else {");
                    builder.AppendTabLine(4, "throw new UnauthorizedException('Invalid access token');");
                    builder.AppendTabLine(3, "}");
                    builder.AppendTabLine(2, "} else {");
                    builder.AppendTabLine(3, "throw new UnauthorizedException('Required access token');");
                    builder.AppendTabLine(2, "}");
                }

                builder.AppendTabLine(1, "}");
                if (!option.UsingCustomModel)
                {
                    builder.AppendLine("}");
                }
            }

            return builder.ToString();
        }
    }
}
