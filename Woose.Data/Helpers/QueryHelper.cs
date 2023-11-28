using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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
                    /*
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
                    */
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
                    /*
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
                    */
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
                    /*
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
                    */
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
                    /*
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
                    */
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
                    /*
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
                    */
                    result.Append($"Group by  {this.Columns} ");
                    break;
            }
            return result.ToString();
        }

        public IFeedback ToResult()
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
                    default:
                        var rtn = new ReturnValues<List<T>>();
                        var tmp = this.Command.ExecuteEntities<T>();
                        if (tmp != null && tmp.Count > 0)
                        {
                            rtn.Success(tmp.Count, tmp);
                        }
                        else
                        {
                            rtn.Success(0, new List<T>());
                        }
                        this.Result = rtn;
                        break;
                }
            }
            else
            {
                var rtn = new ReturnValues<List<T>>();
                var tmp = this.Command.ExecuteEntities<T>();
                if (tmp != null && tmp.Count > 0)
                {
                    rtn.Success(tmp.Count, tmp);
                }
                else
                {
                    rtn.Success(0, new List<T>());
                }
                this.Result = rtn;
            }

            return this.Result;
        }
    }

    public class QueryOption
    {
        public string Column { get; set; } = string.Empty;

        public object? Value { get; set; } = default!;

        public QueryOption() { }

        public QueryOption(string column, object? value)
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
        public static QueryHelper<T> Execute<T>(this QueryHelper<T> helper, SqlCommand? cmd) where T : IEntity, new()
        {
            if (cmd != null)
            {
                cmd.CommandText = helper.ToQuery();
                cmd.CommandType = System.Data.CommandType.Text;
                switch (helper.Method.ToUpper().Trim())
                {
                    case "UPDATE":
                        foreach (var property in helper.target.GetInfo())
                        {
                            var option = (from a in helper.Options where a.Value.Column.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (option != null)
                            {
                                cmd.Parameters.Set($"@{property.ColumnName.Trim()}Value", property.Type, option.Value, property.Size);
                            }
                            object whereValue = (from a in helper.WhereOptions where a.Key.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (whereValue != null)
                            {
                                cmd.Parameters.Set($"@{property.ColumnName}", property.Type, whereValue, property.Size);
                            }
                        }
                        break;
                    case "INSERT":
                        foreach (var property in helper.target.GetInfo())
                        {
                            var option = (from a in helper.Options where a.Value.Column.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (option != null)
                            {
                                cmd.Parameters.Set($"@{property.ColumnName.Trim()}", property.Type, option.Value, property.Size);
                            }
                            object whereValue = (from a in helper.WhereOptions where a.Key.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (whereValue != null)
                            {
                                cmd.Parameters.Set($"@{property.ColumnName}", property.Type, whereValue, property.Size);
                            }
                        }
                        break;
                    default:
                        foreach (var property in helper.target.GetInfo())
                        {
                            object whereValue = (from a in helper.WhereOptions where a.Key.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (whereValue != null)
                            {
                                cmd.Parameters.Set($"@{property.ColumnName}", property.Type, whereValue, property.Size);
                            }
                        }
                        break;
                }

                helper.Command = cmd;
            }

            return helper;
        }


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
            var query = new QueryHelper<T>();
            if (IsDynamicEntity)
            {
                query.Create().NotExists().Execute(cmd);
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

        public static QueryHelper<T> Update<T>(this QueryHelper<T> query, T paramData) where T : IEntity, new()
        {
            query.Method = "Update";

            QueryOption option = new QueryOption();
            foreach (var info in query.GetInfos)
            {
                if (info != null && !string.IsNullOrWhiteSpace(info.ColumnName))
                {
                    if (!info.IsKey)
                    {
                        option = new QueryOption(info.ColumnName, paramData.GetValue(info.ColumnName));
                        query.Options.AddOrUpdate(info.ColumnName.ToUpper().Trim(), option, (x, y) => option);
                    }
                }
            }

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

        public static QueryHelper<T> Where<T>(this QueryHelper<T> query, Expression<Func<T, object>> predicate) where T : IEntity, new()
        {
            if (predicate != null)
            {
                query = TranslateExpressionToQuery(query, predicate);
            }
            return query;
        }

        public static QueryHelper<T> And<T>(this QueryHelper<T> query, Expression<Func<T, object>> predicate) where T : IEntity, new()
        {
            if (predicate != null)
            {
                query = TranslateExpressionToQuery(query, predicate, "and");
            }
            return query;
        }

        public static QueryHelper<T> Or<T>(this QueryHelper<T> query, Expression<Func<T, object>> predicate) where T : IEntity, new()
        {
            if (predicate != null)
            {
                query = TranslateExpressionToQuery(query, predicate, "or");
            }
            return query;
        }


        private static QueryHelper<T> TranslateExpressionToQuery<T>(QueryHelper<T> helper, Expression<Func<T, object>> predicate, string operatorStr = "") where T : IEntity, new()
        {
            Expression body = predicate.Body;

            // UnaryExpression 언랩
            if (body is UnaryExpression unaryExpression)
            {
                body = unaryExpression.Operand;
            }

            if (body is BinaryExpression binaryExpression)
            {
                return TranslateBinaryExpression(helper, binaryExpression.Left, binaryExpression.NodeType, binaryExpression.Right, operatorStr);
            }
            else
            {
                return TranslateBinaryExpression(helper, body, operatorStr);
            }
        }

        private static QueryHelper<T> TranslateBinaryExpression<T>(QueryHelper<T> helper, Expression left, string operatorStr = "") where T : IEntity, new()
        {
            string leftOperand = TranslateOperand(left);

            EntityInfo info = helper.GetInfos.Where(x => x.ColumnName.Equals(leftOperand, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (info != null)
            {
                switch (info.Type)
                {
                    case System.Data.SqlDbType.Bit:
                        if (string.IsNullOrWhiteSpace(helper.Where))
                        {
                            helper.Where = $"{leftOperand} = @{leftOperand}";
                            if (info != null)
                            {
                                helper.WhereOptions.AddOrUpdate(leftOperand, true, (x, y) => true);
                            }
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder(helper.Where);

                            builder.Append($" {((string.IsNullOrWhiteSpace(operatorStr) ? "and" : operatorStr))} {leftOperand} = @{leftOperand}");
                            helper.Where = builder.ToString();

                            helper.WhereOptions.AddOrUpdate(leftOperand, true, (x, y) => true);
                        }
                        break;
                    case System.Data.SqlDbType.VarChar:
                    case System.Data.SqlDbType.Char:
                    case System.Data.SqlDbType.NVarChar:
                    case System.Data.SqlDbType.NChar:
                    case System.Data.SqlDbType.Text:
                    case System.Data.SqlDbType.NText:
                        if (string.IsNullOrWhiteSpace(helper.Where))
                        {
                            helper.Where = $"{leftOperand} <> ''";
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder(helper.Where);

                            builder.Append($" {((string.IsNullOrWhiteSpace(operatorStr) ? "and" : operatorStr))} {leftOperand} <> ''");
                            helper.Where = builder.ToString();
                        }
                        break;
                    case System.Data.SqlDbType.BigInt:
                    case System.Data.SqlDbType.Int:
                    case System.Data.SqlDbType.SmallInt:
                    case System.Data.SqlDbType.TinyInt:
                    case System.Data.SqlDbType.Money:
                    case System.Data.SqlDbType.SmallMoney:
                    case System.Data.SqlDbType.Float:
                    case System.Data.SqlDbType.Real:
                        if (string.IsNullOrWhiteSpace(helper.Where))
                        {
                            helper.Where = $"{leftOperand} > -1";
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder(helper.Where);

                            builder.Append($" {((string.IsNullOrWhiteSpace(operatorStr) ? "and" : operatorStr))} {leftOperand} > -1");
                            helper.Where = builder.ToString();
                        }
                        break;
                    default:
                        break;
                }
            }

            return helper;
        }


        private static QueryHelper<T> TranslateBinaryExpression<T>(QueryHelper<T> helper, Expression left, ExpressionType nodeType, Expression right, string operatorStr = "") where T : IEntity, new()
        {
            string leftOperand = TranslateOperand(left);
            string rightOperand = TranslateOperand(right);
            string operatorString = TranslateBinaryExpressionType(nodeType);

            EntityInfo info = helper.GetInfos.Where(x => x.ColumnName.Equals(leftOperand, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (info != null)
            {
                switch (info.Type)
                {
                    case System.Data.SqlDbType.Bit:
                        if (string.IsNullOrWhiteSpace(helper.Where))
                        {
                            helper.Where = $"{leftOperand} {operatorString} @{leftOperand}";
                            if (info != null)
                            {
                                helper.WhereOptions.AddOrUpdate(leftOperand, rightOperand, (x, y) => rightOperand);
                            }
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder(helper.Where);

                            builder.Append($" {((string.IsNullOrWhiteSpace(operatorStr) ? "and" : operatorStr))} {leftOperand} {operatorString} @{leftOperand}");
                            helper.Where = builder.ToString();

                            helper.WhereOptions.AddOrUpdate(leftOperand, rightOperand, (x, y) => rightOperand);
                        }
                        break;
                    default:
                        if (string.IsNullOrWhiteSpace(helper.Where))
                        {
                            helper.Where = $"{leftOperand} {operatorString} @{leftOperand}";
                            if (info != null)
                            {
                                helper.WhereOptions.AddOrUpdate(leftOperand, rightOperand, (x, y) => rightOperand);
                            }
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder(helper.Where);
                            
                            builder.Append($" {((string.IsNullOrWhiteSpace(operatorStr) ? "and" : operatorStr))} {leftOperand} {operatorString} @{leftOperand}");
                            helper.Where = builder.ToString();

                            helper.WhereOptions.AddOrUpdate(leftOperand, rightOperand, (x, y) => rightOperand);
                        }
                        break;
                }
            }

            return helper;
        }

        private static string TranslateOperand(Expression operand)
        {
            if (operand is MemberExpression memberExpression)
            {
                // 멤버 표현식인 경우 해당 멤버의 이름을 반환
                return memberExpression.Member.Name;
            }
            else if (operand is ConstantExpression constantExpression)
            {
                // 상수 표현식인 경우 상수의 문자열 표현 반환
                return constantExpression.Value.ToString();
            }
            else if (operand is BinaryExpression binaryExpression)
            {
                // 이제 BinaryExpression에 대한 처리 추가
                string left = TranslateOperand(binaryExpression.Left);
                string right = TranslateOperand(binaryExpression.Right);

                switch (binaryExpression.NodeType)
                {
                    case ExpressionType.Equal:
                        return $"{left} = {right}";
                    case ExpressionType.AndAlso:
                        return $"({left} AND {right})";
                    case ExpressionType.NotEqual:
                        return $"({left} <> {right})";
                    default:
                        throw new NotSupportedException($"Unsupported binary expression type: {binaryExpression.NodeType}");
                }
            }
            else
            {
                throw new NotSupportedException($"Unsupported binary expression type: {operand.NodeType}");
            }
        }

        private static string TranslateBinaryExpressionType(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                // 더 많은 비교 연산자에 대한 처리 추가
                default:
                    return " and ";
            }
        }


        public static QueryHelper<T> Where<T>(this QueryHelper<T> query, string ColumnName, object? ColumnValue) where T : IEntity, new()
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
