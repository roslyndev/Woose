using System.Data.SqlClient;

namespace Woose.Data
{
    public interface IContext
    {
        SqlConnection getConnection();
    }
}
