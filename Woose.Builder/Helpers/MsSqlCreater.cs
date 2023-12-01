using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Woose.Builder
{
    public class MsSqlCreater
    {
        public MsSqlCreater()
        {
        }

        public string CreateSaveSP(OptionData options, List<DbTableInfo> info)
        {
            StringBuilder builder = new StringBuilder(200);

            if (info != null && info.Count > 0)
            {
                DbTableInfo primaryKey = info.Where(x => x.is_identity).FirstOrDefault();
                if (primaryKey == null)
                {
                    primaryKey = info.OrderBy(x => x.column_id).FirstOrDefault();
                }

                builder.AppendLine($"CREATE PROCEDURE [dbo].[USP_{info[0].TableName}_Save]");
                builder.AppendLine("(");
                int num = 0;
                foreach (var item in info.Where(x => x.Name != primaryKey?.Name && x.IsDate == false && x.is_nullable == false))
                {
                    if (num > 0)
                    {
                        builder.Append(",");
                    }
                    builder.AppendTabString(1, $"@{item.Name}");
                    builder.AppendTabString((item.Name.Length < 7) ? 4 : 3, item.ColumnType);
                    switch (item.ColumnType)
                    {
                        case "text":
                        case "ntext":
                        case "nvarchar":
                        case "nchar":
                        case "varchar":
                        case "char":
                            if (item.max_length > 0)
                            {
                                builder.Append($"({item.max_length})");
                            }
                            break;
                    }
                    builder.AppendEmptyLine();
                    num++;
                }
                foreach (var item in info.Where(x => x.Name != primaryKey?.Name && x.IsDate == false && x.is_nullable == true))
                {
                    if (num > 0)
                    {
                        builder.Append(",");
                    }
                    builder.AppendTabString(1, $"@{item.Name}");
                    builder.AppendTabString((item.Name.Length < 7) ? 4 : 3, item.ColumnType);
                    switch (item.ColumnType)
                    {
                        case "text":
                        case "ntext":
                        case "nvarchar":
                        case "nchar":
                        case "varchar":
                        case "char":
                            if (item.max_length > 0)
                            {
                                builder.Append($"({item.max_length})");
                            }
                            break;
                    }
                    builder.AppendTabString(1, "= null");
                    builder.AppendEmptyLine();
                    num++;
                }
                if (num > 0)
                {
                    builder.Append(",");
                }
                builder.AppendTabString(1, $"@{primaryKey.Name}");
                builder.AppendTabString((primaryKey.Name.Length < 7) ? 4 : 3, primaryKey.ColumnType);
                switch (primaryKey.ColumnType)
                {
                    case "text":
                    case "ntext":
                    case "nvarchar":
                    case "nchar":
                    case "varchar":
                    case "char":
                        if (primaryKey.max_length > 0)
                        {
                            builder.Append($"({primaryKey.max_length})");
                        }
                        break;
                }
                if (primaryKey.IsNumber)
                {
                    builder.AppendTabString(3, "= -1");
                }
                else
                {
                    builder.AppendTabString(3, "= null");
                }
                builder.AppendEmptyLine();
                if (options.BindModel == OptionData.BindModelType.ReturnValue.ToString())
                {
                    builder.Append(",");
                    builder.AppendTabString(1, $"@Code");
                    builder.AppendTabString(4, "bigint");
                    builder.AppendTabStringLine(2, "output");
                    builder.Append(",");
                    builder.AppendTabString(1, $"@Value");
                    builder.AppendTabString(4, "varchar(100)");
                    builder.AppendTabStringLine(1, "output");
                    builder.Append(",");
                    builder.AppendTabString(1, $"@Msg");
                    builder.AppendTabString(4, "nvarchar(100)");
                    builder.AppendTabStringLine(1, "output");
                }
                builder.AppendEmptyLine();
                builder.AppendLine(")");
                builder.AppendLine("AS");
                builder.AppendEmptyLine();
                builder.AppendLine("SET NOCOUNT ON");
                builder.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                builder.AppendEmptyLine();
                builder.AppendLine("");
                builder.AppendLine("Declare");
                builder.AppendLine("\t@Err\t\t\tint");
                if (options.BindModel == OptionData.BindModelType.ExecuteResult.ToString())
                {
                    builder.AppendLine(",\t@Msg\t\t\tnvarchar(100)");
                    builder.AppendLine(",\t@Code\t\t\tvarchar(100)");
                }
                builder.AppendLine(",\t@idx\t\t\tbigint");
                builder.AppendLine("");
                builder.AppendLine("SET @Err = 0");
                builder.AppendLine("SET @Msg = ''");
                builder.AppendLine("SET @idx = 0");
                builder.AppendLine("");
                builder.Append("IF ");
                if (primaryKey.IsNumber)
                {
                    builder.Append($"IsNull(@{primaryKey.Name},0) > 0 and ");
                }
                else
                {
                    builder.Append($"IsNull(@{primaryKey.Name},'') <> '' and ");
                }
                builder.AppendLine($"Exists (Select {primaryKey.Name} from [{info[0].TableName}] where {primaryKey.Name} = @{primaryKey.Name})");
                builder.AppendTabStringLine(1, "BEGIN");
                builder.AppendTabStringLine(2, $"Update [{info[0].TableName}] set");
                for (int i = 0; i < info.Count; i++)
                {
                    if (info[i].Name != primaryKey.Name && info[i].Name != "RegistDate")
                    {
                        builder.AppendTabString(2, $"[{info[i].Name}]");
                        if (info[i].IsDate)
                        {
                            builder.AppendTabString(3, $"= getdate()");
                        }
                        else
                        {
                            builder.AppendTabString((info[i].Name.Length < 7) ? 4 : 3, $"= @{info[i].Name}");
                        }
                        if (i < info.Count - 1)
                        {
                            builder.AppendLine(",");
                        }
                        else
                        {
                            builder.AppendEmptyLine();
                        }
                    }
                }
                builder.AppendTabStringLine(2, $"where [{primaryKey.Name}] = @{primaryKey.Name}");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, $"SET @Err = @Err + @@Error");
                builder.AppendEmptyLine();
                builder.AppendTabStringLine(2, "IF IsNull(@Err,0) = 0");
                builder.AppendTabStringLine(3, "BEGIN");
                builder.AppendTabStringLine(4, $"SET @idx = @{primaryKey.Name}");
                builder.AppendTabStringLine(3, "END");

                builder.AppendTabStringLine(1, "END");
                builder.AppendLine("ELSE");
                builder.AppendTabStringLine(1, "BEGIN");
                builder.AppendTabStringLine(2, $"Insert into [{info[0].TableName}] (");
                num = 0;
                foreach (var item in info.Where(x => x.Name != primaryKey?.Name))
                {
                    builder.AppendTabString(2, "");
                    if (num > 0)
                    {
                        builder.Append(",");
                    }
                    builder.AppendTabStringLine(1, $"[{item.Name}]");
                    num++;
                }
                if (primaryKey != null && !primaryKey.is_identity)
                {
                    builder.AppendTabString(2, "");
                    if (num > 0)
                    {
                        builder.Append(",");
                    }
                    builder.AppendTabStringLine(1, $"[{primaryKey.Name}]");
                }
                builder.AppendTabStringLine(2, ") values (");
                num = 0;
                foreach (var item in info.Where(x => x.Name != primaryKey?.Name))
                {
                    builder.AppendTabString(2, "");
                    if (num > 0)
                    {
                        builder.Append(",");
                    }
                    if (item.IsDate)
                    {
                        builder.AppendTabStringLine(1, "getdate()");
                    }
                    else
                    {
                        builder.AppendTabStringLine(1, $"@{item.Name}");
                    }
                    num++;
                }
                if (primaryKey != null && !primaryKey.is_identity)
                {
                    builder.AppendTabString(2, "");
                    if (num > 0)
                    {
                        builder.Append(",");
                    }
                    if (primaryKey.IsDate)
                    {
                        builder.AppendTabStringLine(1, "getdate()");
                    }
                    else
                    {
                        builder.AppendTabStringLine(1, $"@{primaryKey.Name}");
                    }
                }
                builder.AppendTabStringLine(2, ")");
                if (primaryKey != null && primaryKey.is_identity)
                {
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(2, "SET @idx = @@IDENTITY");
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(2, "IF Not(IsNull(@idx,0) > 0)");
                    builder.AppendTabStringLine(3, "BEGIN");
                    builder.AppendTabStringLine(4, "SET @Err = @Err + 1");
                    builder.AppendTabStringLine(3, "END");
                }
                else
                {
                    builder.AppendEmptyLine();
                    builder.AppendTabStringLine(2, "SET @Err = @Err + @@Error");
                }
                builder.AppendTabStringLine(1, "END");

                builder.AppendLine("");
                if (options.BindModel == OptionData.BindModelType.ExecuteResult.ToString())
                {
                    builder.AppendLine("Select");
                    builder.AppendLine("\t@Err as [IsError]");
                    builder.AppendLine(",\t@Msg as [Message]");
                    builder.AppendLine(",\t@Code as [Code]");
                    builder.AppendLine(",\t@idx as [TargetIDX]");
                }
                else
                {
                    builder.AppendLine("Set @Code = @idx");
                }
            }

            return builder.ToString();
        }
    }
}
