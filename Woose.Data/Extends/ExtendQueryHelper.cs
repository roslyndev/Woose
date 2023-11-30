using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Woose.Core;

namespace Woose.Data
{
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

        public static QueryHelper<T> Set<T>(this QueryHelper<T> helper) where T : IEntity, new()
        {
            if (helper.Command != null)
            {
                helper.Command.CommandText = helper.ToQuery();
                helper.Command.CommandType = System.Data.CommandType.Text;
                switch (helper.Method.ToUpper().Trim())
                {
                    case "UPDATE":
                        foreach (var property in helper.target.GetInfo())
                        {
                            var option = (from a in helper.Options where a.Value.Column.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (option != null)
                            {
                                helper.Command.Parameters.Set($"@{property.ColumnName.Trim()}Value", property.Type, option.Value, property.Size);
                            }
                            object whereValue = (from a in helper.WhereOptions where a.Key.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (whereValue != null)
                            {
                                helper.Command.Parameters.Set($"@{property.ColumnName}", property.Type, whereValue, property.Size);
                            }
                        }
                        break;
                    case "INSERT":
                        foreach (var property in helper.target.GetInfo())
                        {
                            var option = (from a in helper.Options where a.Value.Column.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (option != null)
                            {
                                helper.Command.Parameters.Set($"@{property.ColumnName.Trim()}", property.Type, option.Value, property.Size);
                            }
                            object whereValue = (from a in helper.WhereOptions where a.Key.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (whereValue != null)
                            {
                                helper.Command.Parameters.Set($"@{property.ColumnName}", property.Type, whereValue, property.Size);
                            }
                        }
                        break;
                    default:
                        foreach (var property in helper.target.GetInfo())
                        {
                            object whereValue = (from a in helper.WhereOptions where a.Key.Trim().ToUpper() == property.ColumnName.Trim().ToUpper() select a.Value).FirstOrDefault();
                            if (whereValue != null)
                            {
                                helper.Command.Parameters.Set($"@{property.ColumnName}", property.Type, whereValue, property.Size);
                            }
                        }
                        break;
                }
            }

            return helper;
        }

        public static QueryHelper<T> Void<T>(this QueryHelper<T> helper) where T : IEntity, new()
        {
            helper.Command.ExecuteNonQuery();
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

        public static QueryHelper<T> Paging<T>(this QueryHelper<T> query, int pagesize, int curpage) where T : IEntity, new()
        {
            query.Method = "Paging";
            query.Columns = "*";
            query.CurPage = curpage;
            query.TopCount = pagesize;
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

        public static QueryHelper<T> Insert<T>(this QueryHelper<T> query, T paramData) where T : IEntity, new()
        {
            query.Method = "Insert";
            StringBuilder columns = new StringBuilder(200);
            StringBuilder values = new StringBuilder(200);
            int num = 0;
            foreach (var info in query.GetInfos)
            {
                if (!info.IsKey)
                {
                    if (num > 0)
                    {
                        columns.Append(",");
                        values.Append(",");
                    }
                    columns.Append($"[{info.ColumnName}]");
                    values.Append($"@{info.ColumnName}");
                    num++;
                }
            }
            query.Insert<T>(columns.ToString(), values.ToString());
            foreach (var info in query.GetInfos)
            {
                if (!info.IsKey)
                {
                    query.WhereOptions.AddOrUpdate(info.ColumnName, paramData.GetValue(info.ColumnName), (x, y) => paramData.GetValue(info.ColumnName));
                }
            }
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
            foreach (var item in query.GetInfos.Where(x => !x.IsKey))
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
                foreach (string s in wherestring)
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
