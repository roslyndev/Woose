using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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

                helper.isSet = true;
            }

            return helper;
        }

        public static QueryHelper<T> Void<T>(this QueryHelper<T> helper) where T : IEntity, new()
        {
            helper.Command.ExecuteNonQuery();
            return helper;
        }

        public static QueryHelper<T> NotExists<T>(this QueryHelper<T> helper) where T : IEntity, new()
        {
            helper.IsExists = true;
            return helper;
        }

        public static QueryHelper<T> NotExists<T>(this QueryHelper<T> helper, long primaryKeyValue) where T : IEntity, new()
        {
            helper.IsExists = true;
            helper.PrimaryKeyValue = primaryKeyValue;
            return helper;
        }

        public static QueryHelper<T> Create<T>(this QueryHelper<T> helper) where T : IEntity, new()
        {
            helper.Method = "Create";
            return helper;
        }

        public static QueryHelper<T> Select<T>(this QueryHelper<T> helper) where T : IEntity, new()
        {
            helper.Method = "Select";
            helper.Columns = "*";
            return helper;
        }

        public static QueryHelper<T> Paging<T>(this QueryHelper<T> helper, int pagesize, int curpage) where T : IEntity, new()
        {
            helper.Method = "Paging";
            helper.Columns = "*";
            helper.CurPage = curpage;
            helper.TopCount = pagesize;
            return helper;
        }

        public static QueryHelper<T> Select<T>(this QueryHelper<T> helper, int Count) where T : IEntity, new()
        {
            helper.Method = "Select";
            helper.Columns = "*";
            helper.TopCount = Count;
            return helper;
        }

        public static QueryHelper<T> Update<T>(this QueryHelper<T> helper, T paramData) where T : IEntity, new()
        {
            helper.Method = "Update";

            QueryOption option = new QueryOption();
            foreach (var info in helper.GetInfos)
            {
                if (info != null && !string.IsNullOrWhiteSpace(info.ColumnName))
                {
                    if (!info.IsKey)
                    {
                        option = new QueryOption(info.ColumnName, paramData.GetValue(info.ColumnName));
                        helper.Options.AddOrUpdate(info.ColumnName.ToUpper().Trim(), option, (x, y) => option);
                    }
                }
            }

            return helper;
        }

        public static QueryHelper<T> Insert<T>(this QueryHelper<T> helper, string column, object value) where T : IEntity, new()
        {
            helper.Method = "Insert";
            var option = new QueryOption(column, value);
            helper.Options.AddOrUpdate(column.ToUpper().Trim(), option, (x, y) => option);
            return helper;
        }

        public static QueryHelper<T> Insert<T>(this QueryHelper<T> helper, T paramData) where T : IEntity, new()
        {
            helper.Method = "Insert";
            StringBuilder columns = new StringBuilder(200);
            StringBuilder values = new StringBuilder(200);
            int num = 0;
            foreach (var info in helper.GetInfos)
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
            helper.Insert<T>(columns.ToString(), values.ToString());
            foreach (var info in helper.GetInfos)
            {
                if (!info.IsKey)
                {
                    helper.WhereOptions.AddOrUpdate(info.ColumnName, paramData.GetValue(info.ColumnName), (x, y) => paramData.GetValue(info.ColumnName));
                }
            }
            return helper;
        }

        public static QueryHelper<T> Delete<T>(this QueryHelper<T> helper) where T : IEntity, new()
        {
            helper.Method = "Delete";
            return helper;
        }

        public static QueryHelper<T> Where<T>(this QueryHelper<T> helper, params string[] wherestring) where T : IEntity, new()
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
                helper.Where += (string.IsNullOrWhiteSpace(helper.Where)) ? builder.ToString() : $" and {builder.ToString()}";
            }
            return helper;
        }

        public static QueryHelper<T> Where<T>(this QueryHelper<T> helper, Expression<Func<T, object>> predicate) where T : IEntity, new()
        {
            if (predicate != null)
            {
                helper = TranslateExpressionToQuery(helper, predicate);
            }
            return helper;
        }

        public static QueryHelper<T> And<T>(this QueryHelper<T> helper, Expression<Func<T, object>> predicate) where T : IEntity, new()
        {
            if (predicate != null)
            {
                helper = TranslateExpressionToQuery(helper, predicate, "and");
            }
            return helper;
        }

        public static QueryHelper<T> Or<T>(this QueryHelper<T> helper, Expression<Func<T, object>> predicate) where T : IEntity, new()
        {
            if (predicate != null)
            {
                helper = TranslateExpressionToQuery(helper, predicate, "or");
            }
            return helper;
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

        public static QueryHelper<T> Where<T>(this QueryHelper<T> helper, string ColumnName, object? ColumnValue) where T : IEntity, new()
        {
            helper.WhereOptions.AddOrUpdate(ColumnName, ColumnValue, (x, y) => ColumnValue);
            return helper;
        }

        public static QueryHelper<T> Count<T>(this QueryHelper<T> helper) where T : IEntity, new()
        {
            helper.Method = "Count";
            return helper;
        }

        public static QueryHelper<T> OrderBy<T>(this QueryHelper<T> helper, QueryOption.Sequence order) where T : IEntity, new()
        {
            helper.OrderColumn = helper.PrimaryColumn;
            helper.OrderType = order;

            if (string.IsNullOrWhiteSpace(helper.OrderByString))
            {
                helper.OrderByString = $"{helper.PrimaryColumn} {order.ToString()}";
            }
            else
            {
                StringBuilder builder = new StringBuilder(helper.OrderByString);
                builder.Append($" {helper.PrimaryColumn} {order.ToString()}");
                helper.OrderByString = builder.ToString();
            }

            return helper;
        }

        public static QueryHelper<T> OrderBy<T>(this QueryHelper<T> helper, string Column, QueryOption.Sequence order) where T : IEntity, new()
        {
            helper.OrderColumn = Column;
            helper.OrderType = order;

            if (string.IsNullOrWhiteSpace(helper.OrderByString))
            {
                helper.OrderByString = $"{Column} {order.ToString()}";
            }
            else
            {
                StringBuilder builder = new StringBuilder(helper.OrderByString);
                builder.Append($" {Column} {order.ToString()}");
                helper.OrderByString = builder.ToString();
            }

            return helper;
        }

        public static QueryHelper<T> OrderBy<T>(this QueryHelper<T> helper, Expression<Func<T, object>> predicate, QueryOption.Sequence order) where T : IEntity, new()
        {
            if (predicate != null)
            {
                helper = TranslateExpressionToOrderBy(helper, predicate, order);
            }
            return helper;
        }

        private static QueryHelper<T> TranslateExpressionToOrderBy<T>(QueryHelper<T> helper, Expression<Func<T, object>> predicate, QueryOption.Sequence order) where T : IEntity, new()
        {
            Expression body = predicate.Body;

            // UnaryExpression 언랩
            if (body is UnaryExpression unaryExpression)
            {
                body = unaryExpression.Operand;
            }

            return TranslateBinaryExpression(helper, body, order);
        }

        private static QueryHelper<T> TranslateBinaryExpression<T>(QueryHelper<T> helper, Expression left, QueryOption.Sequence order) where T : IEntity, new()
        {
            string leftOperand = TranslateOperand(left);

            if (string.IsNullOrWhiteSpace(helper.OrderByString))
            {
                helper.OrderByString = $"{leftOperand} {order.ToString()}";
            }
            else
            {
                StringBuilder builder = new StringBuilder(helper.OrderByString);
                builder.Append($" {leftOperand} {order.ToString()}");
                helper.OrderByString = builder.ToString();
            }

            return helper;
        }

        public static QueryHelper<T> GroupBy<T>(this QueryHelper<T> helper, params string[] columns) where T : IEntity, new()
        {
            helper.Method = "Select";

            if (columns != null && columns.Length > 0)
            {
                helper.Method = "Group";

                int num = 0;
                StringBuilder builder = new StringBuilder(200);
                foreach (string column in columns)
                {
                    if (num > 0) builder.Append(",");
                    builder.Append(column);
                    num++;
                }

                helper.Columns = builder.ToString();
            }

            return helper;
        }

    }
}
