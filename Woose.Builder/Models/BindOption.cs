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

        public Database Database { get; set; }

        public BindOption()
        {
            this.Binder = new CodeHelper(this);
        }

        
    }
}
