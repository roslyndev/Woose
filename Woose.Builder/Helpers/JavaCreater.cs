using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Woose.Builder
{
    public class JavaCreater
    {
        public JavaCreater()
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

                builder.Append($"public class {info[0].TableName} ");
                builder.AppendLine("{");
                foreach(var item in info)
                {
                    builder.AppendTabLine(1, $"private {item.JavaType} {item.ColumnName};");
                }
                builder.AppendEmptyLine();
                builder.AppendTab(1, $"public {info[0].TableName}()");
                builder.AppendLine(" {");
                builder.AppendTabLine(1, "}");
                builder.AppendEmptyLine();
                foreach (var item in info)
                {
                    builder.AppendTab(1, $"public void set{item.ColumnName}({item.JavaType} {item.ColumnName.FirstCharToLower()})");
                    builder.Append(" { ");
                    builder.Append($"this.{item.ColumnName} = {item.ColumnName.FirstCharToLower()};");
                    builder.AppendLine(" }");

                    builder.AppendTab(1, $"public {item.JavaType} get{item.ColumnName}()");
                    builder.Append(" { ");
                    builder.Append($"return this.{item.ColumnName};");
                    builder.AppendLine(" }");
                    builder.AppendEmptyLine();
                }

                builder.AppendLine("}");
            }

            return builder.ToString();
        }
    }
}
