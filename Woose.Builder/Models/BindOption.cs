namespace Woose.Builder
{
    public class BindOption
    {
        public string targetType { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public DbEntity target { get; set; } = new DbEntity();

        public CodeHelper Binder { get; set; } = new CodeHelper();

        public BindOption()
        {
            this.Binder = new CodeHelper(this);
        }
    }
}
