namespace Woose.API
{
    public interface IPagingParameter
    {
        int CurPage { get; set; }

        void AddWhere(string where);

        string ToWhereString();
    }
}
