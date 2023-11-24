using System;
using System.Collections.Generic;
using System.Text;
using Woose.Core;

namespace Woose.Data
{
    public class BaseRepository : IRepository
    {
        public BaseRepository() 
        { 
        }

        public List<T> Select<T>() where T : IEntity, new()
        {
            var result = new List<T>();
            T target = new T();

            StringBuilder query = new StringBuilder(200);
            query.Append($"Select * from {target.GetTableName()} with (nolock)");



            return result;
        }
    }
}
