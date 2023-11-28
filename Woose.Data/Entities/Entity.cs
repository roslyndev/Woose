using System;
using System.Text;
using Woose.Core;

namespace Woose.Data
{
    public class Entity<T> where T : IEntity, new()
    {
        private static readonly Lazy<T> aes = new Lazy<T>(() => new T());
        public static T Query { get { return aes.Value; } }

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
}
