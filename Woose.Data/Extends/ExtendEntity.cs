using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Woose.Core;

namespace Woose.Data
{
    public static class ExtendEntity
    {

        public static QueryHelper<T> Select<T>(this T target) where T : IEntity, new()
        {
            var result = new QueryHelper<T>();
            result.Select<T>();
            return result;
        }

        public static QueryHelper<T> Select<T>(this T target, int count) where T : IEntity, new()
        {
            var result = new QueryHelper<T>();
            result.Select<T>(count);
            return result;
        }

        public static QueryHelper<T> Paging<T>(this T target, int pagesize, int curpage) where T : IEntity, new()
        {
            var result = new QueryHelper<T>();
            result.Paging<T>(pagesize, curpage);
            return result;
        }

        public static QueryHelper<T> Count<T>(this T target) where T : IEntity, new()
        {
            var result = new QueryHelper<T>();
            result.Count<T>();
            return result;
        }

        public static QueryHelper<T> Insert<T>(this T target, T paramData) where T : IEntity, new()
        {
            var result = new QueryHelper<T>();
            StringBuilder columns = new StringBuilder(200);
            StringBuilder values = new StringBuilder(200);
            int num = 0;
            foreach (var info in result.GetInfos)
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
            result.Insert<T>(columns.ToString(), values.ToString());
            foreach (var info in result.GetInfos)
            {
                if (!info.IsKey)
                {
                    result.WhereOptions.AddOrUpdate(info.ColumnName, paramData.GetValue(info.ColumnName), (x, y) => paramData.GetValue(info.ColumnName));
                }
            }
            return result;
        }

        public static QueryHelper<T> Delete<T>(this T target) where T : IEntity, new()
        {
            var result = new QueryHelper<T>();
            result.Delete<T>();
            return result;
        }

        public static QueryHelper<T> Update<T>(this T target, T paramData) where T : IEntity, new()
        {
            var result = new QueryHelper<T>();
            result.Update<T>(paramData);
            return result;
        }
    }
}
