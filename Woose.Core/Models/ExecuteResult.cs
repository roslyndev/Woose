using System;

namespace Woose.Core
{
    public class ExecuteResult : BaseResult, IFeedback
    {
        public bool IsSuccess
        {
            get
            {
                return (this.IsError > 0) ? false : true;
            }
        }

        public int IsError { get; set; } = -1;

        public long TargetIDX { get; set; } = -1;

        public string Code { get; set; } = string.Empty;

        public ExecuteResult() : base() 
        {
            this.Type = ResultType.DeclareSelect;
        }

        public BaseResult.ResultType GetResultType()
        {
            return this.Type;
        }

        public void Success(long referenceID, string code = "", string message = "")
        {
            this.IsError = 0;
            this.TargetIDX = referenceID;
            this.Code = code;
            this.Message = message;
        }

        public void Success(long id)
        {
            this.IsError = 0;
            this.TargetIDX = id;
        }

        public void Error(string msg)
        {
            this.IsError = 1;
            this.Message = msg;
        }

        public void Error(Exception ex)
        {
            this.IsError = 1;
            this.Message = ex.Message;
            if (ex.InnerException != null)
            {
                this.Message += "\r\n" + ex.InnerException.Message;
            }
        }

        public ApiResult<ExecuteResult> ToResult()
        {
            var result = new ApiResult<ExecuteResult>();

            if (this.IsSuccess)
            {
                result.Success(this);
            }
            else
            {
                result.Fail(this.Message);
            }

            return result;
        }
    }
}
