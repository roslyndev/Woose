namespace Woose.API
{
    public interface IPagingParameter
    {
        int PageSize { get; set; }

        int CurPage { get; set; }

        void AddWhere(string where);

        string ToWhereString();
    }
}
