using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Woose.Core;

namespace Woose.Data
{
    public class QueryHelper<T> where T : IEntity, new()
    {
        protected StringBuilder query = new StringBuilder(200);

        protected T target { get; set; }

        public string Method { get; set; } = string.Empty;

        public string Columns { get; set; } = string.Empty;

        public string Where { get; set; } = string.Empty;

        public bool IsExists { get; set; } = false;

        public bool IsWrap { get; set; } = false;

        public IFeedback Result { get; set; }

        public long PrimaryKeyValue { get; set; } = -1;

        public ConcurrentDictionary<string, QueryOption> Options { get; set; } = new ConcurrentDictionary<string, QueryOption>();

        public ConcurrentDictionary<string, object> WhereOptions { get; set; } = new ConcurrentDictionary<string, object>();

        public int TopCount { get; set; } = -1;

        public string OrderColumn { get; set; } = string.Empty;

        public QueryOption.Sequence OrderType { get; set; } = QueryOption.Sequence.DESC;

        protected SqlCommand cmd { get; set; }

        public QueryHelper(SqlCommand _cmd) 
        {
            this.target = new T();
            this.cmd = _cmd;
        }

        public List<EntityInfo> GetInfos
        {
            get
            {
                return target.GetInfo();
            }
        }

        public void SetTarget(T _target)
        {
            this.target = _target;
        }

        public object GetValue(string columnName)
        {
            return target.GetValue(columnName);
        }

        public void Append(string query)
        {
            this.query.Append(query);
        }

        public string TableName
        {
            get
            {
                return this.target.GetTableName();
            }
        }

        public string PrimaryColumn
        {
            get
            {
                return this.target.GetPaimaryColumn();
            }
        }

        public void Set()
        {
            this.cmd.CommandText = this.ToQuery();
            this.cmd.CommandType = System.Data.CommandType.Text;
            switch (this.Method.ToUpper().Trim())
            {
                case "UPDATE":
                case "INSERT":
                    foreach(var property in this.target.GetInfo())
                    {
                        var option = (from a in this.Options where a.Value.Column.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                        if (option != null)
                        {
                            this.cmd.Parameters.Set($"@{property.ColumnName}", property.Type, option.Value, property.Size);
                        }
                        object whereValue = (from a in this.WhereOptions where a.Key.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                        if (whereValue != null)
                        {
                            this.cmd.Parameters.Set($"@{property.ColumnName}", property.Type, whereValue, property.Size);
                        }
                    }
                    break;
                default:
                    foreach (var property in this.target.GetInfo())
                    {
                        object whereValue = (from a in this.WhereOptions where a.Key.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                        if (whereValue != null)
                        {
                            this.cmd.Parameters.Set($"@{property.ColumnName}", property.Type, whereValue, property.Size);
                        }
                    }
                    break;
            }
        }

        public void ToResult<U>() where U : IFeedback, new()
        {
            this.IsWrap = true;
            this.Result = new U();
            this.Set();
        }


        public string ToQuery()
        {
            StringBuilder result = new StringBuilder(200);
            int num = 0;
            bool IsWhere = false;
            switch (this.Method.ToUpper().Trim())
            {
                case "CREATE":
                    if (this.IsExists)
                    {
                        result.AppendLine($"IF Not Exists (select 'TABLE' as ObjectType, [object_id],[name] from sys.tables where [name] <> '__RefactorLog' and [name] = '{this.TableName}')");
                        result.AppendLine("BEGIN");
                    }
                    result.AppendLine($"CREATE TABLE [dbo].[{this.TableName}]");
                    result.AppendLine("(");
                    var primary = this.target.GetInfo().Where(x => x.IsKey).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(primary.ColumnName))
                    {
                        result.Append($"    [{primary.ColumnName}] BIGINT NOT NULL PRIMARY KEY IDENTITY");
                    }
                    var arr = this.target.GetInfo().Where(x => !x.IsKey);
                    result.AppendLine((arr.Count() > 0) ? "," : "");
                    num = 0;
                    foreach (var item in arr)
                    {
                        result.Append($"    [{item.ColumnName}] {item.TypeString} NULL");
                        result.AppendLine((num < arr.Count() - 1) ? "," : "");
                        num++;
                    }
                    result.AppendLine(")");
                    if (this.IsExists)
                    {
                        result.AppendLine("END");
                    }
                    break;
                case "SELECT":
                    result.Append("Select ");
                    if (this.TopCount > 0)
                    {
                        result.Append($"Top {this.TopCount} ");
                    }
                    result.Append($"{(string.IsNullOrWhiteSpace(this.Columns) ? "*" : this.Columns)} ");
                    result.Append($"From [{this.TableName}] with (nolock) ");
                    IsWhere = false;
                    if (!string.IsNullOrWhiteSpace(this.Where))
                    {
                        result.Append($"Where {this.Where} ");
                        IsWhere = true;
                    }

                    if (this.WhereOptions.Count > 0)
                    {
                        result.Append((IsWhere) ? " And " : " Where ");
                        num = 0;
                        foreach(var op in this.WhereOptions)
                        {
                            result.Append((num > 0) ? " And " : "");
                            result.Append($"[{op.Key}] = @{op.Key}");
                            num++;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(this.OrderColumn))
                    {
                        result.Append($"Order by {this.OrderColumn} {this.OrderType.ToString()}");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(this.PrimaryColumn))
                        {
                            result.Append($"Order by {this.PrimaryColumn} {this.OrderType.ToString()}");
                        }
                    }
                    break;
                case "UPDATE":
                    if (this.IsWrap)
                    {
                        result.AppendLine($"Declare @rows bigint, @msg nvarchar(100)");
                        result.AppendLine($"SET @rows = 0");
                        result.AppendLine($"SET @msg = ''");
                        result.AppendLine($"");
                        result.AppendLine($"BEGIN TRY");
                    }
                    result.Append($"Update [{this.TableName}] set ");
                    num = 0;
                    foreach(QueryOption item in this.Options.Values)
                    {
                        if (num > 0) result.Append(",");
                        result.Append($"[{item.Column}] = @{item.Column} ");
                        num++;
                    }
                    IsWhere = false;
                    if (!string.IsNullOrWhiteSpace(this.Where))
                    {
                        result.Append($"Where {this.Where} ");
                        IsWhere = true;
                    }

                    if (this.WhereOptions.Count > 0)
                    {
                        result.Append((IsWhere) ? " And " : " Where ");
                        num = 0;
                        foreach (var op in this.WhereOptions)
                        {
                            result.Append((num > 0) ? " And " : "");
                            result.Append($"[{op.Key}] = @{op.Key}");
                            num++;
                        }
                    }
                    if (this.IsWrap)
                    {
                        result.AppendLine($"");
                        result.AppendLine($"SET @rows = @@ROWCOUNT");
                        result.AppendLine($"END TRY");
                        result.AppendLine($"BEGIN CATCH");
                        result.AppendLine($"SET @rows = -1");
                        result.AppendLine($"SET @msg = ERROR_MESSAGE()");
                        result.AppendLine($"END CATCH");

                        switch (this.Result.GetResultType())
                        {
                            case BaseResult.ResultType.DeclareSelect:
                                result.AppendLine("");
                                result.AppendLine("Select");
                                result.AppendLine("(case when @rows > 0 then 0 else 1 end) as [IsError]");
                                result.AppendLine(",@msg as [Message]");
                                break;
                            case BaseResult.ResultType.OutputParameter:
                                result.AppendLine("");
                                result.AppendLine("SET @Code = @rows");
                                break;
                        }
                    }
                    break;
                case "INSERT":
                    if (this.IsExists)
                    {
                        if (this.PrimaryKeyValue > 0)
                        {
                            result.AppendLine($"IF Not Exists (Select [{this.PrimaryColumn}] from [{this.TableName}] where {this.PrimaryColumn}={this.PrimaryKeyValue})");
                        }
                        else
                        {
                            result.AppendLine($"IF Not Exists (Select [{this.PrimaryColumn}] from [{this.TableName}] where {this.Where})");
                        }
                        result.AppendLine("BEGIN");
                    }
                    if (this.IsWrap)
                    {
                        result.AppendLine($"Declare @rows bigint, @msg nvarchar(100)");
                        result.AppendLine($"SET @rows = 0");
                        result.AppendLine($"SET @msg = ''");
                        result.AppendLine($"");
                        result.AppendLine($"BEGIN TRY");
                    }
                    result.Append($"Insert into [{this.TableName}] (");
                    num = 0;
                    foreach (QueryOption item in this.Options.Values)
                    {
                        if (num > 0) result.Append(",");
                        result.Append($"[{item.Column}]");
                        num++;
                    }
                    result.Append(") values (");
                    num = 0;
                    foreach (QueryOption item in this.Options.Values)
                    {
                        if (num > 0) result.Append(",");
                        result.Append($"@{item.Column}");
                        num++;
                    }
                    result.AppendLine(")");
                    if (this.IsWrap)
                    {
                        result.AppendLine($"");
                        result.AppendLine($"SET @rows = @@ROWCOUNT");
                        result.AppendLine($"END TRY");
                        result.AppendLine($"BEGIN CATCH");
                        result.AppendLine($"SET @rows = -1");
                        result.AppendLine($"SET @msg = ERROR_MESSAGE()");
                        result.AppendLine($"END CATCH");

                        switch (this.Result.GetResultType())
                        {
                            case BaseResult.ResultType.DeclareSelect:
                                result.AppendLine("");
                                result.AppendLine("Select");
                                result.AppendLine("(case when @rows > 0 then 0 else 1 end) as [IsError]");
                                result.AppendLine(",@msg as [Message]");
                                break;
                            case BaseResult.ResultType.OutputParameter:
                                result.AppendLine("");
                                result.AppendLine("SET @Code = @rows");
                                break;
                        }
                    }
                    if (this.IsExists)
                    {
                        result.AppendLine("END");
                    }
                    break;
                case "DELETE":
                    result.Append("Delete ");
                    result.Append($"From [{this.TableName}] ");
                    IsWhere = false;
                    if (!string.IsNullOrWhiteSpace(this.Where))
                    {
                        result.Append($"Where {this.Where} ");
                        IsWhere = true;
                    }

                    if (this.WhereOptions.Count > 0)
                    {
                        result.Append((IsWhere) ? " And " : " Where ");
                        num = 0;
                        foreach (var op in this.WhereOptions)
                        {
                            result.Append((num > 0) ? " And " : "");
                            result.Append($"[{op.Key}] = @{op.Key}");
                            num++;
                        }
                    }
                    break;
                case "COUNT":
                    result.Append("Select Count(1) as [Count] ");
                    result.Append($"{(string.IsNullOrWhiteSpace(this.Columns) ? "*" : this.Columns)} ");
                    result.Append($"From [{this.TableName}] with (nolock) ");
                    IsWhere = false;
                    if (!string.IsNullOrWhiteSpace(this.Where))
                    {
                        result.Append($"Where {this.Where} ");
                        IsWhere = true;
                    }

                    if (this.WhereOptions.Count > 0)
                    {
                        result.Append((IsWhere) ? " And " : " Where ");
                        num = 0;
                        foreach (var op in this.WhereOptions)
                        {
                            result.Append((num > 0) ? " And " : "");
                            result.Append($"[{op.Key}] = @{op.Key}");
                            num++;
                        }
                    }
                    break;
                case "GROUP":
                    result.Append("Select ");
                    if (this.TopCount > 0)
                    {
                        result.Append($"Top {this.TopCount} ");
                    }
                    result.Append($"{this.Columns} ");
                    result.Append($"From [{this.TableName}] with (nolock) ");
                    IsWhere = false;
                    if (!string.IsNullOrWhiteSpace(this.Where))
                    {
                        result.Append($"Where {this.Where} ");
                        IsWhere = true;
                    }

                    if (this.WhereOptions.Count > 0)
                    {
                        result.Append((IsWhere) ? " And " : " Where ");
                        num = 0;
                        foreach (var op in this.WhereOptions)
                        {
                            result.Append((num > 0) ? " And " : "");
                            result.Append($"[{op.Key}] = @{op.Key}");
                            num++;
                        }
                    }
                    result.Append($"Group by  {this.Columns} ");
                    break;
            }
            return result.ToString();
        }

    }

    public class QueryOption
    {
        public string Column { get; set; } = string.Empty;

        public object Value { get; set; } = default!;

        public QueryOption() { }

        public QueryOption(string column, object value)
        {
            this.Column = column;
            this.Value = value;
        }

        public enum Sequence
        {
            ASC, DESC
        }
    }

    public static class ExtendQueryHelper
    {
        public static QueryHelper<T> NotExists<T>(this QueryHelper<T> query) where T : IEntity, new()
        {
            query.IsExists = true;
            return query;
        }

        public static QueryHelper<T> NotExists<T>(this QueryHelper<T> query, long primaryKeyValue) where T : IEntity, new()
        {
            query.IsExists = true;
            query.PrimaryKeyValue = primaryKeyValue;
            return query;
        }

        public static QueryHelper<T> CreateQuery<T>(this SqlCommand cmd, bool IsDynamicEntity = false) where T : IEntity, new() 
        {
            var query = new QueryHelper<T>(cmd);
            if (IsDynamicEntity)
            {
                query.Create().NotExists().Set();
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            return query;
        }

        public static QueryHelper<T> Create<T>(this QueryHelper<T> query) where T : IEntity, new()
        {
            query.Method = "Create";
            return query;
        }

        public static QueryHelper<T> Select<T>(this QueryHelper<T> query) where T : IEntity, new()
        {
            query.Method = "Select";
            query.Columns = "*";
            return query;
        }

        public static QueryHelper<T> Select<T>(this QueryHelper<T> query, int Count) where T : IEntity, new()
        {
            query.Method = "Select";
            query.Columns = "*";
            query.TopCount = Count;
            return query;
        }

        public static QueryHelper<T> Update<T>(this QueryHelper<T> query, string column, object value) where T : IEntity, new()
        {
            query.Method = "Update";
            var option = new QueryOption(column, value);
            query.Options.AddOrUpdate(column.ToUpper().Trim(), option, (x, y) => option);
            return query;
        }

        public static QueryHelper<T> Insert<T>(this QueryHelper<T> query, string column, object value) where T : IEntity, new()
        {
            query.Method = "Insert";
            var option = new QueryOption(column, value);
            query.Options.AddOrUpdate(column.ToUpper().Trim(), option, (x, y) => option);
            return query;
        }

        public static QueryHelper<T> UpdateAll<T>(this QueryHelper<T> query, T target) where T : IEntity, new()
        {
            query.Method = "Update";
            query.SetTarget(target);
            foreach (var item in query.GetInfos.Where(x => !x.IsKey))
            {
                var option = new QueryOption(item.ColumnName, query.GetValue(item.ColumnName));
                query.Options.AddOrUpdate(item.ColumnName.ToUpper().Trim(), option, (x, y) => option);
            }
            foreach (var item in query.GetInfos.Where(x => x.IsKey))
            {
                var option = new QueryOption(item.ColumnName, query.GetValue(item.ColumnName));
                query.Where(item.ColumnName, query.GetValue(item.ColumnName));
            }
            return query;
        }


        public static QueryHelper<T> InsertAll<T>(this QueryHelper<T> query, T target) where T : IEntity, new()
        {
            query.Method = "Insert";
            query.SetTarget(target);
            foreach(var item in query.GetInfos.Where(x => !x.IsKey))
            {
                var option = new QueryOption(item.ColumnName, query.GetValue(item.ColumnName));
                query.Options.AddOrUpdate(item.ColumnName.ToUpper().Trim(), option, (x, y) => option);
            }
            return query;
        }

        public static QueryHelper<T> Delete<T>(this QueryHelper<T> query) where T : IEntity, new()
        {
            query.Method = "Delete";
            return query;
        }

        public static QueryHelper<T> Where<T>(this QueryHelper<T> query, params string[] wherestring) where T : IEntity, new()
        {
            if (wherestring != null && wherestring.Length > 0)
            {
                int num = 0;
                StringBuilder builder = new StringBuilder(100);
                foreach(string s in wherestring)
                {
                    builder.Append((num > 0) ? " and " : "");
                    builder.Append(s);
                    num++;
                }
                query.Where += (string.IsNullOrWhiteSpace(query.Where)) ? builder.ToString() : $" and {builder.ToString()}";
            }
            return query;
        }

        public static QueryHelper<T> Where<T>(this QueryHelper<T> query, string ColumnName, object ColumnValue) where T : IEntity, new()
        {
            query.WhereOptions.AddOrUpdate(ColumnName, ColumnValue, (x, y) => ColumnValue);
            return query;
        }

        public static QueryHelper<T> Count<T>(this QueryHelper<T> query) where T : IEntity, new()
        {
            query.Method = "Count";
            return query;
        }

        public static QueryHelper<T> OrderBy<T>(this QueryHelper<T> query, QueryOption.Sequence order) where T : IEntity, new()
        {
            query.OrderColumn = query.PrimaryColumn;
            query.OrderType = order;
            return query;
        }

        public static QueryHelper<T> OrderBy<T>(this QueryHelper<T> query, string Column, QueryOption.Sequence order) where T : IEntity, new()
        {
            query.OrderColumn = Column;
            query.OrderType = order;
            return query;
        }

        public static QueryHelper<T> GroupBy<T>(this QueryHelper<T> query, params string[] columns) where T : IEntity, new()
        {
            query.Method = "Select";

            if (columns != null && columns.Length > 0)
            {
                query.Method = "Group";

                int num = 0;
                StringBuilder builder = new StringBuilder(200);
                foreach (string column in columns)
                {
                    if (num > 0) builder.Append(",");
                    builder.Append(column);
                    num++;
                }

                query.Columns = builder.ToString();
            }
            
            return query;
        }
    }
}
