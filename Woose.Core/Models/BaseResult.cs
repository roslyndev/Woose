namespace Woose.Core
{
    public class BaseResult
    {
        public string Message { get; set; } = string.Empty;

        public ResultType Type { get; set; } = ResultType.Unknown;

        public BaseResult()
        {

        }

        public enum ResultType
        { 
            DeclareSelect, OutputParameter, Unknown
        }

    }
}
