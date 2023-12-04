using System;
using System.Data;
using System.Reflection;

namespace Woose.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EntityAttribute : Attribute
    {
        public EntityInfo info { get; set; } = new EntityInfo();

        public EntityAttribute(string columnName, SqlDbType dbType, int size = -1, bool isKey = false)
        {
            this.info.ColumnName = columnName;
            this.info.Type = dbType;
            this.info.Size = size;
            this.info.IsKey = isKey;
        }


        public EntityAttribute(string columnName, SqlDbType dbType, bool isKey = false)
        {
            this.info.ColumnName = columnName;
            this.info.Type = dbType;
            this.info.Size = -1;
            switch (dbType)
            {
                case SqlDbType.BigInt:
                    this.info.Size = 8;
                    break;
                case SqlDbType.Money:
                case SqlDbType.Int:
                    this.info.Size = 4;
                    break;
                case SqlDbType.SmallMoney:
                case SqlDbType.SmallInt:
                    this.info.Size = 2;
                    break;
                case SqlDbType.Bit:
                case SqlDbType.TinyInt:
                    this.info.Size = 1;
                    break;
            }
            this.info.IsKey = isKey;
        }
    }

    public static class ExtendEntity
    {
        public static EntityInfo? GetColumnInfo<T>(this T enumValue) where T : struct
        {
            EntityInfo? result = null;

            Type type = enumValue.GetType();
            if (type.IsPublic)
            {
                FieldInfo fi = type.GetField(enumValue.ToString());
                EntityAttribute[]? attrs = fi.GetCustomAttributes(typeof(EntityAttribute), false) as EntityAttribute[];
                if (attrs != null && attrs.Length > 0)
                {
                    result = attrs[0].info;
                }
            }

            return result;
        }
    }
}
