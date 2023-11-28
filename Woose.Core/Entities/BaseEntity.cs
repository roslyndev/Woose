using System.Collections.Generic;
using System.Reflection;
using System;

namespace Woose.Core
{
    public abstract class BaseEntity : IEntity
    {
        protected string TableName { get; set; } = default!;

        protected string PrimaryColumn { get; set; } = default!;

        public virtual void SetPaimaryColumn(string primary)
        {
            this.PrimaryColumn = primary;
        }

        public virtual void SetTableName(string tableName)
        {
            this.TableName = tableName;
        }

        public virtual string GetPaimaryColumn()
        {
            return this.PrimaryColumn;
        }

        public virtual string GetTableName()
        {
            return this.TableName;
        }

        public virtual List<EntityInfo> GetInfo()
        {
            List<EntityInfo> infoList = new List<EntityInfo>();

            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                EntityAttribute[]? attrs = property.GetCustomAttributes(typeof(EntityAttribute), false) as EntityAttribute[];

                if (attrs != null && attrs.Length > 0)
                {
                    // 엔터티 속성에 대한 정보 수집
                    EntityInfo propertyInfo = attrs[0].info;
                    infoList.Add(propertyInfo);
                }
            }

            return infoList;
        }

        public object? GetValue(string propertyName)
        {
            Type type = this.GetType();
            PropertyInfo property = type.GetProperty(propertyName);

            if (property != null)
            {
                return property.GetValue(this);
            }

            return null;
        }
    }
}
