using System;

namespace Woose.Core
{
    public class ApiResult<T> : BaseResult, IFeedback
    {
        public bool IsSuccess { get; set; } = false;


        public long Code { get; set; } = 0;

        public int Count { get; set; } = 0;

        public string Value { get; set; } = string.Empty;

        public T Data { get; set; } = default!;

        public ApiResult() : base()
        {
            this.Type = ResultType.Unknown;
        }

        public BaseResult.ResultType GetResultType()
        {
            return this.Type;
        }

        public void Success()
        {
            this.IsSuccess = true;
            this.Code = 0;
        }

        public void Success(T data)
        {
            this.IsSuccess = true;
            this.Code = 0;
            this.Data = data;
        }

        public void Success(long code)
        {
            this.IsSuccess = true;
            this.Code = code;
        }

        public void Success(string _value)
        {
            this.IsSuccess = true;
            this.Value = _value;
        }

        public void Fail(string msg)
        {
            this.IsSuccess = false;
            this.Code = 1;
            this.Message = msg;
        }

        public void Fail(Exception ex)
        {
            this.IsSuccess = false;
            this.Code = 2;
            this.Message = ex.Message;
        }

        public void Error(string msg)
        {
            this.IsSuccess = false;
            this.Code = 1;
            this.Message = msg;
        }

        public void Error(Exception ex)
        {
            this.IsSuccess = false;
            this.Code = 2;
            this.Message = ex.Message;
        }
    }
}
