using System.Collections.Concurrent;

namespace Woose.Builder
{
    public class BindOption
    {
        public string ProjectName { get; set; } = string.Empty;

        public string MethodName { get; set; } = string.Empty;

        public string targetType { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public DbEntity target { get; set; } = new DbEntity();

        public List<DbEntity> tables { get; set; } = new List<DbEntity>();

        public List<DbEntity> sps { get; set; } = new List<DbEntity>();

        public ConcurrentDictionary<string, List<DbTableInfo>> tableProperties { get; set; } = new ConcurrentDictionary<string, List<DbTableInfo>>();

        public ConcurrentDictionary<string, List<SPEntity>> spProperties { get; set; } = new ConcurrentDictionary<string, List<SPEntity>>();

        public ConcurrentDictionary<string, List<SpOutput>> spOutputs { get; set; } = new ConcurrentDictionary<string, List<SpOutput>>();

        public ConcurrentDictionary<string, List<SpTable>> spTables { get; set; } = new ConcurrentDictionary<string, List<SpTable>>();

        public CodeHelper Binder { get; set; } = new CodeHelper();
        
        public string ReturnType { get; set; } = string.Empty;

        public string BindModel { get; set; } = string.Empty;
        public bool IsNoModel { get; set; } = false;

        public bool UsingCustomModel { get; set; } = false;

        public bool Usei18n { get; set; } = false;

        public bool UseMultiApi { get; set; } = false;

        public string ReturnModel { get; set; } = string.Empty;

        public bool IsAsync { get; set; } = false;

        public string MethodType { get; set; } = "HttpPost";

        public string BindModelIsBoolean
        {
            get
            {
                if (BindModel.Equals("ExecuteResult", StringComparison.OrdinalIgnoreCase))
                {
                    return "isSuccess";
                }
                else
                {
                    return "check";
                }
            }
        }

        public string BindModelCount
        {
            get
            {
                if (BindModel.Equals("ExecuteResult", StringComparison.OrdinalIgnoreCase))
                {
                    return "count";
                }
                else
                {
                    return "code";
                }
            }
        }

        public string BindModelResult
        {
            get
            {
                if (BindModel.Equals("ExecuteResult", StringComparison.OrdinalIgnoreCase))
                {
                    return "ApiResult";
                }
                else
                {
                    return "ReturnValues";
                }
            }
        }

        public BindOption()
        {
            this.Binder = new CodeHelper(this);
        }

        public List<DbTableInfo> GetTableProperties(string tableName)
        {
            List<DbTableInfo>? result = null;

            if (tableProperties != null && tableProperties.TryGetValue(tableName, out result))
            {
                return result ?? new List<DbTableInfo>();
            }

            return new List<DbTableInfo>();
        }

        public List<SPEntity> GetSpProperties(string spName)
        {
            List<SPEntity>? result = null;

            if (spProperties != null && spProperties.TryGetValue(spName, out result))
            {
                return result ?? new List<SPEntity>();
            }

            return new List<SPEntity>();
        }

        public List<SpOutput> GetSpOutputs(string spName)
        {
            List<SpOutput>? result = null;

            if (spOutputs != null && spOutputs.TryGetValue(spName, out result))
            {
                return result ?? new List<SpOutput>();
            }

            return new List<SpOutput>();
        }

        public List<SpTable> GetSpTables(string spName)
        {
            List<SpTable>? result = null;

            if (spTables != null && spTables.TryGetValue(spName, out result))
            {
                return result ?? new List<SpTable>();
            }

            return new List<SpTable>();
        }

        public DbEntity? Find(List<DbEntity> tables, List<SpOutput> spoutput)
        {
            if (tables != null && tables.Count > 0)
            {
                try
                {
                    int num = 0;
                    int i = 0;

                    foreach (var table in tables)
                    {
                        if (table != null)
                        {
                            try
                            {
                                num = 0;
                                i = 0;
                                var properties = this.GetTableProperties(table.name);
                                if (properties != null)
                                {
                                    foreach (var info in properties)
                                    {
                                        num += (spoutput.Where(x => x.name.Equals(info.ColumnName, StringComparison.OrdinalIgnoreCase)).Count() > 0) ? 1 : 0;
                                        i++;
                                    }

                                    if (num == i)
                                    {
                                        return table;
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch
                {

                }
            }

            return null;
        }

        public DbEntity? Find(List<SpOutput> spoutput)
        {
            if (tables != null && tables.Count > 0)
            {
                try
                {
                    int num = 0;
                    int i = 0;

                    foreach (var table in tables)
                    {
                        if (table != null)
                        {
                            try
                            {
                                num = 0;
                                i = 0;
                                var properties = this.GetTableProperties(table.name);
                                if (properties != null)
                                {
                                    foreach (var info in properties)
                                    {
                                        num += (spoutput.Where(x => x.name.Equals(info.ColumnName, StringComparison.OrdinalIgnoreCase)).Count() > 0) ? 1 : 0;
                                        i++;
                                    }

                                    if (num == i)
                                    {
                                        return table;
                                    }
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

    }
}
