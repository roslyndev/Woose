using System.Collections.Generic;

namespace Woose.Core
{
    public interface IEntity
    {
        void SetTableName(string tableName);

        void SetPaimaryColumn(string primary);

        string GetPaimaryColumn();
        string GetTableName();

        List<EntityInfo> GetInfo();

        object? GetValue(string propertyName);
    }
}
