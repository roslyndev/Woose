using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Woose.Core;

namespace Woose.Data
{
    public class DynamicQueryHelper<T> : QueryHelper where T : IEntity, new()
    {
        public T Target { get; set; }

        protected bool IsTopCount { get; set; } = false;

        protected int TopCount { get; set; } = 10;

        protected int CurPage { get; set; } = 1;

        protected ILogHelper Logger { get; set; }

        protected bool IsTryCatch { get; set; } = false;

        protected string WhereString { get; set; } = string.Empty;

        protected string OrderColumn { get; set; } = string.Empty;

        protected ConcurrentDictionary<string, object> WhereParameter = new ConcurrentDictionary<string, object>();

        protected ConcurrentDictionary<string, object> ModelValue = new ConcurrentDictionary<string, object>();

        protected QueryOption.Sequence OrderBy { get; set; } = QueryOption.Sequence.DESC;

        public DynamicQueryHelper() 
        {
            this.Target = new T();
        }

        public string Columns
        {
            get
            {
                if (this.Target != null)
                {
                    return String.Join(',', from a in this.Target.GetInfo() select $"[{a.ColumnName}]");
                }
                else
                {
                    return "*";
                }
            }
        }

        public string PrimaryColumn
        {
            get
            {
                if (this.Target != null)
                {
                    return this.Target.GetPaimaryColumn();
                }
                else
                {
                    return "";
                }
            }
        }

        public string OrderType
        {
            get
            {
                if (this.Target != null)
                {
                    if (!string.IsNullOrWhiteSpace(this.OrderColumn))
                    {
                        return $"{this.OrderColumn} {((this.OrderBy == QueryOption.Sequence.ASC) ? "asc" : "desc")}";
                    }
                    else
                    {
                        return $"{this.PrimaryColumn} {((this.OrderBy == QueryOption.Sequence.ASC) ? "asc" : "desc")}";
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        public DynamicQueryHelper(T paramData)
        {
            this.Target = paramData;
        }

        public DynamicQueryHelper<T> Select()
        {
            this.Method = "Select";
            this.IsTopCount = false;
            return this;
        }

        public DynamicQueryHelper<T> Select(int topCount)
        {
            this.Method = "Select";
            this.IsTopCount = true;
            this.TopCount = topCount;
            return this;
        }

        public DynamicQueryHelper<T> Count()
        {
            this.Method = "Count";
            this.IsTopCount = false;
            return this;
        }

        public DynamicQueryHelper<T> SetLogger(ILogHelper logger)
        {
            this.Logger = logger;
            return this;
        }

        public DynamicQueryHelper<T> Insert(params string[] columns)
        {
            this.Method = "Insert";
            this.IsTopCount = false;

            if (columns != null && columns.Length > 0)
            {
                foreach(string column in columns)
                {
                    EntityInfo? info = this.Target.GetInfo().Where(x => x.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase) && x.IsKey == false).FirstOrDefault();
                    if (info != null && !string.IsNullOrWhiteSpace(info.ColumnName))
                    {
                        ModelValue.AddOrUpdate(info.ColumnName, this.Target.GetValue(info.ColumnName), (n, v) => this.Target.GetValue(info.ColumnName));
                    }
                }
            }

            return this;
        }

        public DynamicQueryHelper<T> Insert()
        {
            this.Method = "Insert";
            this.IsTopCount = false;

            foreach (var info in this.Target.GetInfo().Where(x => x.IsKey == false))
            {
                if (info != null && !string.IsNullOrWhiteSpace(info.ColumnName))
                {
                    ModelValue.AddOrUpdate(info.ColumnName, this.Target.GetValue(info.ColumnName), (n, v) => this.Target.GetValue(info.ColumnName));
                }
            }

            return this;
        }

        public DynamicQueryHelper<T> Update(params string[] columns)
        {
            this.Method = "Update";
            this.IsTopCount = false;

            if (columns != null && columns.Length > 0)
            {
                foreach (string column in columns)
                {
                    EntityInfo? info = this.Target.GetInfo().Where(x => x.ColumnName.Equals(column, StringComparison.OrdinalIgnoreCase) && x.IsKey == false).FirstOrDefault();
                    if (info != null && !string.IsNullOrWhiteSpace(info.ColumnName))
                    {
                        ModelValue.AddOrUpdate(info.ColumnName, this.Target.GetValue(info.ColumnName), (n, v) => this.Target.GetValue(info.ColumnName));
                    }
                }
            }

            return this;
        }

        public DynamicQueryHelper<T> Update()
        {
            this.Method = "Update";
            this.IsTopCount = false;

            foreach (var info in this.Target.GetInfo().Where(x => x.IsKey == false))
            {
                if (info != null && !string.IsNullOrWhiteSpace(info.ColumnName))
                {
                    ModelValue.AddOrUpdate(info.ColumnName, this.Target.GetValue(info.ColumnName), (n, v) => this.Target.GetValue(info.ColumnName));
                }
            }

            return this;
        }

        public DynamicQueryHelper<T> Delete()
        {
            this.Method = "Delete";
            this.IsTopCount = false;
            return this;
        }

        public DynamicQueryHelper<T> Try()
        {
            this.IsTryCatch = true;
            return this;
        }

        public DynamicQueryHelper<T> Paging(int topCount, int curpage = 1)
        {
            this.Method = "Paging";
            this.IsTopCount = true;
            this.TopCount = topCount;
            this.CurPage = curpage;
            return this;
        }

        public DynamicQueryHelper<T> Where(string whereStr)
        {
            this.WhereString = whereStr;
            return this;
        }

        public DynamicQueryHelper<T> Where(string columnName, object columnValue)
        {
            WhereParameter.AddOrUpdate(columnName, columnValue, (n, v) => columnValue);
            return this;
        }

        public DynamicQueryHelper<T> And(string whereStr)
        {
            StringBuilder builder = new StringBuilder(this.WhereString);
            builder.Append($" and {whereStr}");
            return this;
        }

        public DynamicQueryHelper<T> And(string columnName, object columnValue)
        {
            WhereParameter.AddOrUpdate(columnName, columnValue, (n, v) => columnValue);
            return this;
        }

        public DynamicQueryHelper<T> Asc(string columnName)
        {
            this.OrderColumn = columnName;
            this.OrderBy = QueryOption.Sequence.ASC;
            return this;
        }

        public DynamicQueryHelper<T> Desc(string columnName)
        {
            this.OrderColumn = columnName;
            this.OrderBy = QueryOption.Sequence.DESC;
            return this;
        }

        public override void Set()
        {
            this.Command.CommandText = this.ToQuery();
            if (this.Logger != null)
            {
                this.Logger.Debug($"Query : {this.Command.CommandText}");
            }
            this.Command.CommandType = System.Data.CommandType.Text;
            if (ModelValue != null && ModelValue.Count > 0)
            {
                foreach (var item in ModelValue)
                {
                    EntityInfo? info = this.Target.GetInfo().Where(x => x.ColumnName.Equals(item.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (info != null && !string.IsNullOrWhiteSpace(info.ColumnName))
                    {
                        this.Command.Parameters.Set(info.ColumnName, info.Type, item.Value, info.Size);
                    }
                }
            }
            if (WhereParameter != null && WhereParameter.Count > 0)
            {
                foreach (var item in WhereParameter)
                {
                    EntityInfo? info = this.Target.GetInfo().Where(x => x.ColumnName.Equals(item.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (info != null && !string.IsNullOrWhiteSpace(info.ColumnName))
                    {
                        this.Command.Parameters.Set($"{info.ColumnName}Where", info.Type, item.Value, info.Size);
                    }
                }
            }
        }

        public string ToQuery()
        {
            StringBuilder builder = new StringBuilder(200);
            int num = 0;

            switch (this.Method)
            {
                case "Select":
                    builder.Append("Select");
                    if (this.IsTopCount)
                    {
                        builder.Append($" Top {this.TopCount}");
                    }
                    builder.Append($" * from [{this.Target.GetTableName()}] with (nolock)");
                    if (this.TryToWhereString(out string where1))
                    {
                        builder.Append($" where {where1}");
                    }
                    break;
                case "Count":
                    builder.Append($"Select 1 as [Count] from [{this.Target.GetTableName()}] with (nolock)");
                    if (this.TryToWhereString(out string where2))
                    {
                        builder.Append($" where {where2}");
                    }
                    break;
                case "Insert":
                    if (this.IsTryCatch)
                    {
                        builder.AppendLine("Declare @Err int, @Code bigint, @Value varchar(100), @Msg nvarchar(100)");
                        builder.AppendLine("SET @Err = 0");
                        builder.AppendLine("SET @Code = -1");
                        builder.AppendLine("SET @Value = ''");
                        builder.AppendLine("SET @Msg = ''");
                        builder.AppendLine("");
                        builder.AppendLine("BEGIN TRY");
                    }
                    builder.Append($"Insert into [{this.Target.GetTableName()}]");
                    if (ModelValue != null && ModelValue.Count > 0)
                    {
                        builder.Append(" (");
                        num = 0;
                        foreach (var item in ModelValue)
                        {
                            if (num > 0) builder.Append(",");
                            builder.Append($"[{item.Key}]");
                            num++;
                        }
                        builder.Append(") values (");
                        num = 0;
                        foreach (var item in ModelValue)
                        {
                            if (num > 0) builder.Append(",");
                            builder.Append($"@{item.Key}");
                            num++;
                        }
                        builder.AppendLine(")");
                    }
                    if (this.IsTryCatch)
                    {
                        builder.AppendLine("SET @Code = @@IDENTITY");
                        builder.AppendLine("END TRY");
                        builder.AppendLine("BEGIN CATCH");
                        builder.AppendLine("SET @Code = -1");
                        builder.AppendLine("SET @Msg = ERROR_MESSAGE()");
                        builder.AppendLine("END CATCH");
                        builder.AppendLine("");
                        builder.AppendLine("Select @Err as IsError, @Code as [TargetIDX], @Msg as [Message], @Value as [Code]");
                    }
                    break;
                case "Update":
                    if (this.IsTryCatch)
                    {
                        builder.AppendLine("Declare @Err int, @Code bigint, @Value varchar(100), @Msg nvarchar(100)");
                        builder.AppendLine("SET @Err = 0");
                        builder.AppendLine("SET @Code = -1");
                        builder.AppendLine("SET @Value = ''");
                        builder.AppendLine("SET @Msg = ''");
                        builder.AppendLine("");
                        builder.AppendLine("BEGIN TRY");
                    }
                    builder.AppendLine($"Update [{this.Target.GetTableName()}] set");
                    if (ModelValue != null && ModelValue.Count > 0)
                    {
                        num = 0;
                        foreach (var item in ModelValue)
                        {
                            builder.Append(((num > 0) ? "," : " "));
                            builder.Append($"[{item.Key}] = @{item.Key}");
                            num++;
                        }
                    }
                    if (this.TryToWhereString(out string where3))
                    {
                        builder.AppendLine($" where {where3}");
                    }
                    if (this.IsTryCatch)
                    {
                        builder.AppendLine("SET @Code = @@ROWCOUNT");
                        builder.AppendLine("END TRY");
                        builder.AppendLine("BEGIN CATCH");
                        builder.AppendLine("SET @Code = -1");
                        builder.AppendLine("SET @Msg = ERROR_MESSAGE()");
                        builder.AppendLine("END CATCH");
                        builder.AppendLine("");
                        builder.AppendLine("Select @Err as IsError, @Code as [TargetIDX], @Msg as [Message], @Value as [Code]");
                    }
                    break;
                case "Delete":
                    if (this.IsTryCatch)
                    {
                        builder.AppendLine("Declare @Err int, @Code bigint, @Value varchar(100), @Msg nvarchar(100)");
                        builder.AppendLine("SET @Err = 0");
                        builder.AppendLine("SET @Code = -1");
                        builder.AppendLine("SET @Value = ''");
                        builder.AppendLine("SET @Msg = ''");
                        builder.AppendLine("");
                        builder.AppendLine("BEGIN TRY");
                    }
                    builder.AppendLine($"Delete from [{this.Target.GetTableName()}]");
                    if (this.TryToWhereString(out string where4))
                    {
                        builder.AppendLine($" where {where4}");
                    }
                    if (this.IsTryCatch)
                    {
                        builder.AppendLine("SET @Code = @@ROWCOUNT");
                        builder.AppendLine("END TRY");
                        builder.AppendLine("BEGIN CATCH");
                        builder.AppendLine("SET @Code = -1");
                        builder.AppendLine("SET @Msg = ERROR_MESSAGE()");
                        builder.AppendLine("END CATCH");
                        builder.AppendLine("");
                        builder.AppendLine("Select @Err as IsError, @Code as [TargetIDX], @Msg as [Message], @Value as [Code]");
                    }
                    break;
                case "Paging":
                    builder.Append($"SELECT TOP {this.TopCount} resultTable.* FROM (");
                    builder.Append($"SELECT TOP ({this.TopCount} * {this.CurPage}) ROW_NUMBER () OVER (ORDER BY {this.OrderType}) AS rownumber,");
                    builder.Append($"{(string.IsNullOrWhiteSpace(this.Columns) ? "*" : this.Columns)} ");
                    builder.Append($"From [{this.Target.GetTableName()}] with (nolock) ");
                    builder.Append($") AS resultTable ");
                    builder.Append($"WHERE rownumber > ({this.CurPage} - 1) * {this.TopCount} ");
                    if (this.TryToWhereString(out string where5))
                    {
                        builder.Append($" and {where5}");
                    }
                    break;
                case "Direct":
                    builder = new StringBuilder(SpName);
                    break;
            }

            return builder.ToString();
        }

        protected bool TryToWhereString(out string whereStr)
        {
            whereStr = "";
            StringBuilder builder = new StringBuilder(200);
            int num = 0;
            if (!string.IsNullOrWhiteSpace(this.WhereString))
            {
                num++;
                builder.Append(this.WhereString);
            }
            if (this.WhereParameter != null && this.WhereParameter.Count > 0)
            {
                foreach(var item in this.WhereParameter)
                {
                    if (num > 0) builder.Append(" and");
                    builder.Append($" {item.Key} = @{item.Key}Where");
                    num++;
                }
            }

            whereStr = builder.ToString();

            return (num > 0);
        }
    }

    public class QueryHelper
    {
        public SqlCommand Command { get; set; }

        protected string Method { get; set; } = string.Empty;

        protected string SpName { get; set; } = string.Empty;

        public QueryHelper() 
        { 
        }

        public QueryHelper SP(string spName)
        {
            this.Method = "SP";
            this.SpName = spName;
            return this;
        }

        public QueryHelper Direct(string query)
        {
            this.Method = "Direct";
            this.SpName = query;
            return this;
        }

        public virtual void Set()
        {
            this.Command.CommandText = this.SpName;
            this.Command.CommandType = System.Data.CommandType.StoredProcedure;
        }
    }


    public static class ExtendDynamicQueryHelper
    {
        public static DynamicQueryHelper<T> On<T>(this SqlCommand cmd) where T : IEntity, new()
        {
            var result = new DynamicQueryHelper<T>();
            result.Command = cmd;
            return result;
        }

        public static DynamicQueryHelper<T> On<T>(this SqlCommand cmd, T paramData) where T : IEntity, new()
        {
            var result = new DynamicQueryHelper<T>(paramData);
            result.Command = cmd;
            return result;
        }

        public static QueryHelper On(this SqlCommand cmd, string spName)
        {
            var result = new QueryHelper();
            result.Command = cmd;
            result.SP(spName);
            return result;
        }

        public static QueryHelper Execute(this SqlCommand cmd, string query)
        {
            var result = new QueryHelper();
            result.Command = cmd;
            result.Direct(query);
            return result;
        }
    }
}
