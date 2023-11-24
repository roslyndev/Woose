using System;

namespace Woose.Core
{

    public class ReturnValue : BaseResult, IFeedback
    {
        public bool Check { get; set; } = false;
        public long Code { get; set; } = -1;
        public string Value { get; set; } = string.Empty;
        
        public ReturnValue() : base()
        {
            this.Type = ResultType.OutputParameter;
        }

        public BaseResult.ResultType GetResultType()
        {
            return this.Type;
        }

        public virtual void Success(long code)
        {
            this.Code = code;
            this.Check = true;
        }

        public virtual void Success(long code, string value)
        {
            this.Code = code;
            this.Value = value;
            this.Check = true;
        }

        public virtual void Success(long code, string value, string msg)
        {
            this.Code = code;
            this.Value = value;
            this.Message = msg;
            this.Check = true;
        }

        public virtual void Error(string msg)
        {
            this.Message = msg;
            this.Check = false;
        }

        public virtual void Error(Exception ex)
        {
            if (ex.InnerException != null)
            {
                this.Message = $"{ex.InnerException.Message}({ex.Message}){Environment.NewLine}{ex.InnerException.StackTrace}";
            }
            else
            {
                this.Message = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
            }
            this.Check = false;
        }
    }
}
