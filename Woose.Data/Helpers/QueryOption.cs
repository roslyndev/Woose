namespace Woose.Data
{
    public class QueryOption
    {
        public string Column { get; set; } = string.Empty;

        public object? Value { get; set; } = default!;

        public QueryOption() { }

        public QueryOption(string column, object? value)
        {
            this.Column = column;
            this.Value = value;
        }

        public enum Sequence
        {
            ASC, DESC
        }
    }
}
