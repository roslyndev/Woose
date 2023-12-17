using System;

namespace Woose.Core
{
    public interface ILogHelper
    {
        void Debug(string msg);
        void Error(string msg);
        void Error(Exception ex);
        void Warn(string msg);
        void Fatal(string msg);
        void Info(string msg);
    }
}
