using System;

namespace Woose.Core
{
    public interface IFeedback
    {
        void Error(string message);
        void Error(Exception ex);

        void Success(long num);

        BaseResult.ResultType GetResultType();
    }
}
