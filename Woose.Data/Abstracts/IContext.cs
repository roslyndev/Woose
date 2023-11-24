using System;
using System.Collections.Generic;
using System.Text;

namespace Woose.Data
{
    public interface IContext
    {
        DatabaseConnection getConnection();
    }
}
