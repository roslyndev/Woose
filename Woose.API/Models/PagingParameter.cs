using System.Text;

namespace Woose.API
{
    public class PagingParameter : IPagingParameter
    {
        public int CurPage { get; set; } = 1;

        protected StringBuilder AddWhereString { get; set; } = new StringBuilder(200);

        protected int cnt = 0;

        public PagingParameter()
        {
        }

        public virtual void AddWhere(string where)
        {
            if (cnt > 0)
            {
                this.AddWhereString.Append(" and ");
            }
            this.AddWhereString.Append(where);
            cnt++;
        }

        public virtual string ToWhereString()
        {
            return AddWhereString.ToString();
        }
    }
}
