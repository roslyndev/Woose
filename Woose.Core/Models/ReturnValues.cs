namespace Woose.Core
{
    public class ReturnValues<T> : ReturnValue, IFeedback where T : new()
    {
        public T Data { get; set; } = default!;
        public ReturnValues() : base()
        {
            this.Data = new T();
            this.Type = ResultType.OutputParameter;
        }

        public override BaseResult.ResultType GetResultType()
        {
            return this.Type;
        }

        public virtual void Success(long code, T obj)
        {
            this.Code = code;
            this.Data = obj;
            this.Check = true;
        }

        public virtual void Success(long code, T obj, string value)
        {
            this.Code = code;
            this.Data = obj;
            this.Value = value;
            this.Check = true;
        }

        public virtual void Success(long code, T obj, string value, string msg)
        {
            this.Code = code;
            this.Data = obj;
            this.Value = value;
            this.Message = msg;
            this.Check = true;
        }
    }
}
