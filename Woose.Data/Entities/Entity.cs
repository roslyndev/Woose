using System;
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

        public Entity()
        {
        }

        public Entity Set(string _query)
        {
            this.query = new StringBuilder(_query);
            return this;
        }

        public Entity Execute(SqlCommand cmd)
        {
            this.Command = cmd;
            return this;
        }

        public int Void()
        {
            return this.Command.ExecuteNonQuery();
        }
    }
}
