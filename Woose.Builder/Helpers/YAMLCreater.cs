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

        public string CreateEntity(OptionData options, List<DbTableInfo> info)
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

                builder.AppendTabStringLine(2, $"{mainTable}:");
                builder.AppendTabStringLine(3, "type: object");
                builder.AppendTabStringLine(3, "properties:");
                foreach (var item in info)
                {
                    builder.AppendTabStringLine(4, $"{item.Name.FirstCharToLower()}:");
                    builder.AppendTabStringLine(5, $"type: {item.ScriptType}");
                }
            }

            return builder.ToString();
        }

        public string CreateSP(OptionData options, List<SPEntity> properties, List<SpTable> tables, List<SpOutput> outputs)
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
                builder.AppendTabStringLine(2, $"{mainTable}Output:");
                builder.AppendTabStringLine(3, "type: object");
                builder.AppendTabStringLine(3, "properties:");

                foreach (var item in outputs)
                {
                    builder.AppendTabStringLine(4, $"{item.name.FirstCharToLower()}:");
                    builder.AppendTabStringLine(5, $"type: {DbTypeHelper.MSSQL.GetObjectTypeByYAML(item.system_type_name)}");
                }
                builder.AppendEmptyLine();
            }


            if (properties != null && properties.Count > 0)
            {
                builder.AppendLine($"//입력 파라미터 모델");
                builder.AppendTabStringLine(2, $"{mainTable}Input:");
                builder.AppendTabStringLine(3, "type: object");
                builder.AppendTabStringLine(3, "properties:");
                foreach (var item in properties)
                {
                    builder.AppendTabStringLine(4, $"{item.name.Replace("@","").FirstCharToLower()}:");
                    builder.AppendTabStringLine(5, $"type: {DbTypeHelper.MSSQL.GetObjectTypeByYAML(item.type)}");
                }
            }

            return builder.ToString();
        }
    }
}
