using System;
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

        public T target { get; set; }

        public string Method { get; set; } = string.Empty;

        public string Columns { get; set; } = string.Empty;

        public string Where { get; set; } = string.Empty;

        public bool IsExists { get; set; } = false;

        public bool IsWrap { get; set; } = false;

        public IFeedback? Result { get; set; }

        public long PrimaryKeyValue { get; set; } = -1;

        public ConcurrentDictionary<string, QueryOption> Options { get; set; } = new ConcurrentDictionary<string, QueryOption>();

        public ConcurrentDictionary<string, object?> WhereOptions { get; set; } = new ConcurrentDictionary<string, object?>();

        public int TopCount { get; set; } = -1;

        public int CurPage { get; set; } = 1;

        public string OrderColumn { get; set; } = string.Empty;

        public QueryOption.Sequence OrderType { get; set; } = QueryOption.Sequence.DESC;

        public SqlCommand Command { get; set; }


        public QueryHelper() 
        {
            this.target = new T();
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

        public object? GetValue(string columnName)
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


        public QueryHelper<T> SetResult<U>() where U : IFeedback, new()
        {
            this.IsWrap = true;
            this.Result = new U();
            return this;
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
                case "PAGING":
                    result.Append($"SELECT TOP {this.TopCount} resultTable.* FROM (");
                    result.Append($"SELECT TOP ({this.TopCount} * {this.CurPage}) ROW_NUMBER () OVER (ORDER BY {this.PrimaryColumn} {this.OrderType.ToString()}) AS rownumber,");
                    result.Append($"{(string.IsNullOrWhiteSpace(this.Columns) ? "*" : this.Columns)} ");
                    result.Append($"From [{this.TableName}] with (nolock) ");
                    result.Append($") AS resultTable ");
                    result.Append($"WHERE rownumber > ({this.CurPage} - 1) * {this.TopCount} ");
                    if (!string.IsNullOrWhiteSpace(this.Where))
                    {
                        result.Append($"And {this.Where} ");
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
                        result.Append($"[{item.Column.Trim()}] = @{item.Column.Trim()}Value ");
                        num++;
                    }
                    IsWhere = false;
                    if (!string.IsNullOrWhiteSpace(this.Where))
                    {
                        result.Append($"Where {this.Where} ");
                        IsWhere = true;
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

                        if (this.Result != null)
                        {
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
                        result.Append($"{item.Column}");
                        num++;
                    }
                    result.Append(") values (");
                    num = 0;
                    foreach (QueryOption item in this.Options.Values)
                    {
                        if (num > 0) result.Append(",");
                        result.Append($"{item.Value}");
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

                        if (this.Result != null)
                        {
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
                    break;
                case "COUNT":
                    result.Append("Select Count(1) as [Count] ");
                    result.Append($"From [{this.TableName}] with (nolock) ");
                    IsWhere = false;
                    if (!string.IsNullOrWhiteSpace(this.Where))
                    {
                        result.Append($"Where {this.Where} ");
                        IsWhere = true;
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
                    result.Append($"Group by  {this.Columns} ");
                    break;
            }
            return result.ToString();
        }

        public List<T> ToList()
        {
            return this.Command.ExecuteEntities<T>();
        }

        public T ToEntity()
        {
            return this.Command.ExecuteEntity<T>();
        }

        public int ToCount()
        {
            return Convert.ToInt32(this.Command.ExecuteScalar() ?? 0);
        }

        public IFeedback? ToResult()
        {
            if (this.Result != null)
            {
                switch (this.Result.GetResultType())
                {
                    case BaseResult.ResultType.DeclareSelect:
                        this.Result = this.Command.ExecuteResult();
                        break;
                    case BaseResult.ResultType.OutputParameter:
                        this.Result = this.Command.ExecuteReturnValue();
                        break;
                }
            }

            return this.Result;
        }

    }
}
