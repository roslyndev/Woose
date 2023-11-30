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
        public static T Run { get { return entity.Value; } }

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
        public static Entity Run { get { return entity.Value; } }

        protected StringBuilder query { get; set; } = new StringBuilder(200);

        protected SqlCommand Command { get; set; } = default!;

        protected CommandType ExecuteType { get; set; } = default!;

        public Entity()
        {
        }

        public Entity On(SqlDbOperater handler)
        {
            this.Command = handler.Command;
            return this;
        }

        public Entity Query(string _query)
        {
            this.query = new StringBuilder(_query);
            this.ExecuteType = CommandType.Text;
            return this;
        }

        public Entity Execute(SqlCommand? cmd)
        {
            if (cmd != null)
            {
                this.Command = cmd;
                this.Command.CommandType = this.ExecuteType;
                this.Command.CommandText = this.query.ToString();
            }
            return this;
        }

        public Entity Set()
        {
            if (this.Command != null)
            {
                this.Command.CommandType = this.ExecuteType;
                this.Command.CommandText = this.query.ToString();
            }
            return this;
        }

        public Entity StoredProcedure(string spname)
        {
            this.query = new StringBuilder(spname);
            this.ExecuteType = CommandType.StoredProcedure;
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

        public object ToScalar()
        {
            var dt = this.Command.ExecuteTable();
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0][0];
            }
            else
            {
                return null;
            }
        }

        public Entity SetParameter(string fieldName, SqlDbType type, object fieldValue, int size = -1)
        {
            if (this.Command != null)
            {
                this.Command.Parameters.Set(fieldName, type, fieldValue, size);
            }
            return this;
        }
    }
}
