namespace Woose.Builder
{
    public class OptionData
    {
        public string ReturnType { get; set; } = string.Empty;

        public string ReturnModel { get; set; } = string.Empty;

        public string BindModel { get; set; } = string.Empty;

        public bool UsingCurrentModel { get; set; } = false;

        public bool UsingCustomModel { get; set; } = false;

        public bool IsInLine { get; set; } = false;

        public bool IsNoModel { get; set; } = false;

        public enum BindModelType
        {
            ExecuteResult, ReturnValue
        }

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



        public OptionData()
        {
        }
    }
}
