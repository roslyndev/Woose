using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Woose.Builder
{
    public class YAMLCreater
    {
        public YAMLCreater()
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

                string mainTable = info[0].TableName.Trim().FirstCharToLower();

                builder.AppendSpaceLine(2, $"{mainTable}:");
                builder.AppendSpaceLine(3, "type: object");
                builder.AppendSpaceLine(3, "properties:");
                foreach (var item in info)
                {
                    builder.AppendSpaceLine(4, $"{item.Name.FirstCharToLower()}:");
                    builder.AppendSpaceLine(5, $"type: {item.ScriptType}");
                }
            }

            return builder.ToString();
        }

        public string CreateSP(BindOption options, List<SPEntity> properties, List<SpTable> tables, List<SpOutput> outputs)
        {
            StringBuilder builder = new StringBuilder(200);
            string mainTable = string.Empty;

            if (tables != null && tables.Count > 0)
            {
                mainTable = tables[0].TableName.FirstCharToLower();
            }

            if (outputs != null && outputs.Count > 0)
            {
                builder.AppendLine($"//반환 파라미터 모델");
                builder.AppendSpaceLine(2, $"{mainTable}Output:");
                builder.AppendSpaceLine(3, "type: object");
                builder.AppendSpaceLine(3, "properties:");

                foreach (var item in outputs)
                {
                    builder.AppendSpaceLine(4, $"{item.name.FirstCharToLower()}:");
                    builder.AppendSpaceLine(5, $"type: {DbTypeHelper.MSSQL.GetObjectTypeByYAML(item.system_type_name)}");
                }
                builder.AppendEmptyLine();
            }


            if (properties != null && properties.Count > 0)
            {
                builder.AppendLine($"//입력 파라미터 모델");
                builder.AppendSpaceLine(2, $"{mainTable}Input:");
                builder.AppendSpaceLine(3, "type: object");
                builder.AppendSpaceLine(3, "properties:");
                foreach (var item in properties)
                {
                    builder.AppendSpaceLine(4, $"{item.name.Replace("@","").FirstCharToLower()}:");
                    builder.AppendSpaceLine(5, $"type: {DbTypeHelper.MSSQL.GetObjectTypeByYAML(item.type)}");
                }
            }

            return builder.ToString();
        }

        public string CreateDeclareYaml(BindOption options)
        {
            StringBuilder builder = new StringBuilder(200);

            builder.AppendLine("openapi: 3.0.3");
            builder.AppendLine("info:");
            builder.AppendSpaceLine(1, $"title: {options.ProjectName} Api Server V1");
            builder.AppendSpaceLine(1, "description: >-");
            builder.AppendSpaceLine(2, $"{options.ProjectName} Backend API");
            builder.AppendSpaceLine(1, "contact:");
            builder.AppendSpaceLine(2, "email: admin@yourdomain.com");
            builder.AppendSpaceLine(1, "version: 1.0.0");
            builder.AppendLine("servers:");
            builder.AppendSpaceLine(1, "- url: http://localhost:4000/api");

            return builder.ToString();
        }

        public string CreateApiYaml(BindOption options)
        {
            StringBuilder builder = new StringBuilder(200);

            builder.AppendLine("tags:");
            foreach (var item in options.tables)
            {
                builder.AppendSpaceLine(1, $"- name: {item.name.FirstCharToLower()}");
                builder.AppendSpaceLine(2, $"description: {item.name}");
            }
            builder.AppendEmptyLine();
            builder.AppendLine("paths:");
            foreach (var item in options.tables)
            {
                builder.AppendSpaceLine(1, $"/{item.name.FirstCharToLower()}/list:");
                builder.AppendSpaceLine(2, $"get:");
                builder.AppendSpaceLine(3, $"tags:");
                builder.AppendSpaceLine(4, $"- {item.name.FirstCharToLower()}");
                builder.AppendSpaceLine(3, $"summary: \"{item.name} List\"");
                builder.AppendSpaceLine(3, $"description: \"{item.name} List\"");
                builder.AppendSpaceLine(3, $"parameters:");
                builder.AppendSpaceLine(4, $"- name: curpage");
                builder.AppendSpaceLine(5, $"in: query");
                builder.AppendSpaceLine(5, $"description: \"페이지번호\"");
                builder.AppendSpaceLine(5, $"required: false");
                builder.AppendSpaceLine(5, $"schema:");
                builder.AppendSpaceLine(6, $"type: number");
                builder.AppendSpaceLine(6, $"default: 1");
                builder.AppendSpaceLine(3, $"security:");
                builder.AppendSpaceLine(4, $"- bearer: []");
                builder.AppendSpaceLine(3, $"responses:");
                builder.AppendSpaceLine(4, $"'200':");
                builder.AppendSpaceLine(5, $"description: Successful operation");
                builder.AppendSpaceLine(5, $"content:");
                builder.AppendSpaceLine(6, $"application/json:");
                builder.AppendSpaceLine(7, $"schema:");
                builder.AppendSpaceLine(8, $"$ref: '#/components/schemas/returnValues'");

                builder.AppendSpaceLine(1, $"/{item.name.FirstCharToLower()}/view" + "/{id}:");
                builder.AppendSpaceLine(2, $"get:");
                builder.AppendSpaceLine(3, $"tags:");
                builder.AppendSpaceLine(4, $"- {item.name.FirstCharToLower()}");
                builder.AppendSpaceLine(3, $"summary: \"{item.name} Detail\"");
                builder.AppendSpaceLine(3, $"description: \"{item.name} Detail\"");
                builder.AppendSpaceLine(3, $"parameters:");
                builder.AppendSpaceLine(4, $"- name: id");
                builder.AppendSpaceLine(5, $"in: path");
                builder.AppendSpaceLine(5, $"description: \"identity key\"");
                builder.AppendSpaceLine(5, $"required: true");
                builder.AppendSpaceLine(5, $"schema:");
                builder.AppendSpaceLine(6, $"type: number");
                builder.AppendSpaceLine(6, $"default: -1");
                builder.AppendSpaceLine(3, $"security:");
                builder.AppendSpaceLine(4, $"- bearer: []");
                builder.AppendSpaceLine(3, $"responses:");
                builder.AppendSpaceLine(4, $"'200':");
                builder.AppendSpaceLine(5, $"description: Successful operation");
                builder.AppendSpaceLine(5, $"content:");
                builder.AppendSpaceLine(6, $"application/json:");
                builder.AppendSpaceLine(7, $"schema:");
                builder.AppendSpaceLine(8, $"$ref: '#/components/schemas/returnValues'");

                builder.AppendSpaceLine(1, $"/{item.name.FirstCharToLower()}/erase" + "/{id}:");
                builder.AppendSpaceLine(2, $"delete:");
                builder.AppendSpaceLine(3, $"tags:");
                builder.AppendSpaceLine(4, $"- {item.name.FirstCharToLower()}");
                builder.AppendSpaceLine(3, $"summary: \"{item.name} Remove\"");
                builder.AppendSpaceLine(3, $"description: \"{item.name} Remove\"");
                builder.AppendSpaceLine(3, $"parameters:");
                builder.AppendSpaceLine(4, $"- name: id");
                builder.AppendSpaceLine(5, $"in: path");
                builder.AppendSpaceLine(5, $"description: \"identity key\"");
                builder.AppendSpaceLine(5, $"required: true");
                builder.AppendSpaceLine(5, $"schema:");
                builder.AppendSpaceLine(6, $"type: number");
                builder.AppendSpaceLine(6, $"default: -1");
                builder.AppendSpaceLine(3, $"security:");
                builder.AppendSpaceLine(4, $"- bearer: []");
                builder.AppendSpaceLine(3, $"responses:");
                builder.AppendSpaceLine(4, $"'200':");
                builder.AppendSpaceLine(5, $"description: Successful operation");
                builder.AppendSpaceLine(5, $"content:");
                builder.AppendSpaceLine(6, $"application/json:");
                builder.AppendSpaceLine(7, $"schema:");
                builder.AppendSpaceLine(8, $"$ref: '#/components/schemas/returnValues'");

                builder.AppendSpaceLine(1, $"/{item.name.FirstCharToLower()}/save:");
                builder.AppendSpaceLine(2, $"post:");
                builder.AppendSpaceLine(3, $"tags:");
                builder.AppendSpaceLine(4, $"- {item.name.FirstCharToLower()}");
                builder.AppendSpaceLine(3, $"summary: \"{item.name} Save\"");
                builder.AppendSpaceLine(3, $"description: \"{item.name} Save\"");
                builder.AppendSpaceLine(3, $"requestBody:");
                builder.AppendSpaceLine(4, $"content:");
                builder.AppendSpaceLine(5, $"application/json:");
                builder.AppendSpaceLine(6, $"schema:");
                builder.AppendSpaceLine(7, $"$ref: '#/components/schemas/{item.name}'");
                builder.AppendSpaceLine(3, $"security:");
                builder.AppendSpaceLine(4, $"- bearer: []");
                builder.AppendSpaceLine(3, $"responses:");
                builder.AppendSpaceLine(4, $"'200':");
                builder.AppendSpaceLine(5, $"description: Successful operation");
                builder.AppendSpaceLine(5, $"content:");
                builder.AppendSpaceLine(6, $"application/json:");
                builder.AppendSpaceLine(7, $"schema:");
                builder.AppendSpaceLine(8, $"$ref: '#/components/schemas/returnValues'");
            }
            return builder.ToString();
        }

        public string CreateComponentYaml(BindOption options)
        {
            StringBuilder builder = new StringBuilder(200);

            builder.AppendLine("components:");
            builder.AppendSpaceLine(1, $"securitySchemes:");
            builder.AppendSpaceLine(2, "bearer:");
            builder.AppendSpaceLine(3, $"type: apiKey");
            builder.AppendSpaceLine(3, "scheme: bearer");
            builder.AppendSpaceLine(3, "in: header");
            builder.AppendSpaceLine(3, "name: access_token");
            builder.AppendSpaceLine(1, $"schemas:");
            builder.AppendSpaceLine(2, "returnValue:");
            builder.AppendSpaceLine(3, "type: object");
            builder.AppendSpaceLine(3, "properties:");
            builder.AppendSpaceLine(4, "check:");
            builder.AppendSpaceLine(5, "type: boolean");
            builder.AppendSpaceLine(4, "code:");
            builder.AppendSpaceLine(5, "type: number");
            builder.AppendSpaceLine(4, "value:");
            builder.AppendSpaceLine(5, "type: string");
            builder.AppendSpaceLine(4, "message:");
            builder.AppendSpaceLine(5, "type: string");
            builder.AppendSpaceLine(2, "returnValues:");
            builder.AppendSpaceLine(3, "type: object");
            builder.AppendSpaceLine(3, "properties:");
            builder.AppendSpaceLine(4, "check:");
            builder.AppendSpaceLine(5, "type: boolean");
            builder.AppendSpaceLine(4, "code:");
            builder.AppendSpaceLine(5, "type: number");
            builder.AppendSpaceLine(4, "value:");
            builder.AppendSpaceLine(5, "type: string");
            builder.AppendSpaceLine(4, "message:");
            builder.AppendSpaceLine(5, "type: string");
            builder.AppendSpaceLine(4, "data:");
            builder.AppendSpaceLine(5, "type: object");
            builder.AppendSpaceLine(2, "identity:");
            builder.AppendSpaceLine(3, "type: object");
            builder.AppendSpaceLine(3, "properties:");
            builder.AppendSpaceLine(4, "id:");
            builder.AppendSpaceLine(5, "type: number");

            foreach (var entity in options.tables)
            {
                builder.AppendSpaceLine(2, $"{entity.name.FirstCharToLower()}:");
                builder.AppendSpaceLine(3, "type: object");
                builder.AppendSpaceLine(3, "properties:");
                foreach (var column in options.GetTableProperties(entity.name))
                {
                    builder.AppendSpaceLine(4, $"{column.Name.FirstCharToLower()}:");
                    builder.AppendSpaceLine(5, $"type: {column.ScriptType}");
                }
            }

            return builder.ToString();
        }
    }
}
