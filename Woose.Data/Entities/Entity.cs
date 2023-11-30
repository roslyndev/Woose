using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Woose.Core;

namespace Woose.Data
{
    public class Entity<T> where T : IEntity, new()
    {
        private static readonly Lazy<T> entity = new Lazy<T>(() => new T());
        public static T Query { get { return entity.Value; } }

        protected StringBuilder query { get; set; } = new StringBuilder(200);

        public Entity() 
        { 
        }

        public Entity(string q)
        {
            this.query = new StringBuilder(q);
        }

        public string ToQuery()
        {
            return this.query.ToString();
        }
    }

    public class Entity
    {
        private static readonly Lazy<Entity> entity = new Lazy<Entity>(() => new Entity());
        public static Entity Query { get { return entity.Value; } }

        protected StringBuilder query { get; set; } = new StringBuilder(200);

        protected SqlCommand Command { get; set; } = default!;

        protected CommandType ExecuteType { get; set; } = default!;

        public Entity()
        {
        }

        public Entity Set(string _query)
        {
            this.query = new StringBuilder(_query);
            this.ExecuteType = CommandType.Text;
            return this;
        }

        public Entity Execute(SqlCommand? cmd)
        {
            this.Command = cmd;
            this.Command.CommandType = this.ExecuteType;
            this.Command.CommandText = this.query.ToString();
            return this;
        }

        public int Void()
        {
            return this.Command.ExecuteNonQuery();
        }

        public DataTable ToList()
        {
            return this.Command.ExecuteTable();
        }

        public DataRow? ToEntity()
        {
            var dt = this.Command.ExecuteTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0];
            }
            else
            {
                return null;
            }
        }
    }
}
