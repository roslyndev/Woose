using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
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

        public bool isSet { get; set; } = false;

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
            this.isSet = false;
            return this;
        }

        public Entity Set()
        {
            if (this.Command != null)
            {
                this.Command.CommandType = this.ExecuteType;
                this.Command.CommandText = this.query.ToString();
                this.isSet = true;
            }
            return this;
        }

        public Entity StoredProcedure(string spname)
        {
            this.query = new StringBuilder(spname);
            this.ExecuteType = CommandType.StoredProcedure;
            this.Set();
            return this;
        }

        public int Void()
        {
            if (!this.isSet)
            {
                this.Set();
            }
            return this.Command.ExecuteNonQuery();
        }

        public Task<int> VoidAsync()
        {
            return Task.Factory.StartNew(() => this.Void());
        }

        public DataTable ToList()
        {
            if (!this.isSet)
            {
                this.Set();
            }
            return this.Command.ExecuteTable();
        }

        public Task<DataTable> ToListAsync()
        {
            return Task.Factory.StartNew(() => this.ToList());
        }

        public DataRow? ToEntity()
        {
            if (!this.isSet)
            {
                this.Set();
            }

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

        public Task<DataRow?> ToEntityAsync()
        {
            return Task.Factory.StartNew(() => this.ToEntity());
        }

        public object ToScalar()
        {
            if (!this.isSet)
            {
                this.Set();
            }

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

        public Task<object> ToScalarAsync()
        {
            return Task.Factory.StartNew(() => this.ToScalar());
        }

        public Entity AddParameter(string fieldName, SqlDbType type, object fieldValue, int size = -1)
        {
            if (this.Command != null)
            {
                this.Command.Parameters.Set(fieldName, type, fieldValue, size);
            }
            return this;
        }

        public IFeedback ToResult<T>() where T : IFeedback, new()
        {
            IFeedback result = new T();

            if (!this.isSet)
            {
                this.Set();
            }

            if (this.Command != null)
            {
                switch (result.GetResultType())
                {
                    case BaseResult.ResultType.DeclareSelect:
                        result = this.Command.ExecuteResult();
                        break;
                    case BaseResult.ResultType.OutputParameter:
                        this.Command.Parameters.SetReturnValue();
                        result = this.Command.ExecuteReturnValue();
                        break;
                    default:
                        var dt = this.Command.ExecuteTable();
                        result = EntityHelper.ColumnToEntity<T>(dt);
                        break;
                }
            }


            return result;
        }


    }
}
