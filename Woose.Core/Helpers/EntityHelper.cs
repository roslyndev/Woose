using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Woose.Core
{
    public class EntityHelper
    {
        public static PropertyInfo[] GetProperties<T>()
        {
            Type type = typeof(T);
            return type.GetProperties();
        }

        public static IEntity? CreateInstance(string className)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Type type = assembly.GetType(className);
                if (type != null)
                {
                    return Activator.CreateInstance(type) as IEntity;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool CompareEntity<T,U>(T entity1, U entity2)
        {
            try
            {
                var type1 = typeof(T);
                var type2 = typeof(U);

                var properties1 = type1.GetProperties().Select(p => p.Name);
                var properties2 = type2.GetProperties().Select(p => p.Name);


                return properties2.All(properties1.Contains);
            }
            catch { return false; }
        }

        public static bool CompareEntity<T, U>(List<T> entity1, List<U> entity2)
        {
            try
            {
                var type1 = typeof(T);
                var type2 = typeof(U);

                var properties1 = type1.GetProperties().Select(p => p.Name);
                var properties2 = type2.GetProperties().Select(p => p.Name);


                return properties2.All(properties1.Contains);
            }
            catch { return false; }
        }

        public static List<T> ColumnToEntities<T>(DataTable data) where T : new()
        {
            var result = new List<T>();
            var properties = GetProperties<T>();
            var columns = data.Columns.Cast<DataColumn>().ToList();
            if (columns != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    T item = new T();
                    DataColumn? column;
                    foreach (var property in properties)
                    {
                        try
                        {
                            column = columns.FirstOrDefault(x => x.ColumnName == property.Name);
                            if (column != null && row[property.Name] != null && row[property.Name] != DBNull.Value)
                            {
                                property.SetValue(item, row[property.Name]);
                            }
                        }
                        catch
                        {
                        }
                    }

                    result.Add(item);
                }
            }

            return result;
        }

        public static T ColumnToEntity<T>(DataTable data) where T : new()
        {
            var result = new T();
            var properties = GetProperties<T>();
            var columns = data.Columns.Cast<DataColumn>().ToList();

            DataRow? row = (data.Rows.Count > 0) ? data.Rows[0] : null;
            DataColumn? column;
            if (row != null)
            {
                foreach (var property in properties)
                {
                    try
                    { 
                        column = columns.FirstOrDefault(x => x.ColumnName == property.Name);
                        if (column != null && row[property.Name] != null && row[property.Name] != DBNull.Value)
                        {
                            property.SetValue(result, row[property.Name]);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return result;
        }

        public static List<T> PropertyToEntities<T>(DataTable data) where T : new()
        {
            var result = new List<T>();
            var properties = GetProperties<T>();
            var columns = data.Columns.Cast<DataColumn>().ToList();
            if (columns != null)
            {
                foreach (var property in properties)
                {
                    T item;
                    foreach (DataRow row in data.Rows)
                    {
                        item = new T();
                        DataColumn? column = null;
                        try
                        {
                            column = columns.FirstOrDefault(x => x.ColumnName == property.Name);
                            if (column != null && row[property.Name] != null && row[property.Name] != DBNull.Value)
                            {
                                property.SetValue(item, row[property.Name]);
                            }
                        }
                        catch
                        {
                        }

                        result.Add(item);
                    }
                }
            }

            return result;
        }

        public static T PropertyToEntity<T>(DataTable data) where T : new()
        {
            var result = new T();
            var properties = GetProperties<T>();
            var columns = data.Columns.Cast<DataColumn>().ToList();
            if (columns != null)
            {
                var property = properties[0];
                DataColumn? column = null;
                foreach (DataRow row in data.Rows)
                {
                    try
                    {
                        column = columns.FirstOrDefault(x => x.ColumnName == property.Name);
                        if (column != null && row[property.Name] != null && row[property.Name] != DBNull.Value)
                        {
                            property.SetValue(result, row[property.Name]);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return result;
        }

        public static async Task<List<T>> ColumnToEntitiesAsync<T>(DataTable data) where T : new()
        {
            var result = new List<T>();
            var properties = GetProperties<T>();
            var columns = data.Columns.Cast<DataColumn>().ToList();

            if (columns != null)
            {
                await Task.Run(() =>
                {
                    Parallel.ForEach(data.Rows.Cast<DataRow>(), row =>
                    {
                        T item = new T();
                        DataColumn? column;
                        foreach (var property in properties)
                        {
                            column = columns.FirstOrDefault(x => x.ColumnName == property.Name);
                            if (column != null && row[property.Name] != null && row[property.Name] != DBNull.Value)
                            {
                                property.SetValue(item, row[property.Name]);
                            }
                        }

                        lock (result)
                        {
                            result.Add(item);
                        }
                    });
                });
            }

            return result;
        }

        public static async Task<List<T>> PropertyToEntitiesAsync<T>(DataTable data) where T : new()
        {
            var result = new List<T>();
            var properties = GetProperties<T>();
            var columns = data.Columns.Cast<DataColumn>().ToList();

            if (columns != null)
            {
                await Task.Run(() =>
                {
                    Parallel.ForEach(properties, property =>
                    {
                        T item;
                        foreach (DataRow row in data.Rows)
                        {
                            item = new T();
                            DataColumn? column;
                            try
                            {
                                column = columns.FirstOrDefault(x => x.ColumnName == property.Name);
                                if (column != null && row[property.Name] != null && row[property.Name] != DBNull.Value)
                                {
                                    property.SetValue(item, row[property.Name]);
                                }
                            }
                            catch
                            {
                            }

                            lock (result)
                            {
                                result.Add(item);
                            }
                        }
                    });
                });
            }

            return result;
        }
    }
}
