namespace Woose.Builder
{
    public class BindOption
    {
        public string ProjectName { get; set; } = string.Empty;

        public string MethodName { get; set; } = string.Empty;

        public string targetType { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public DbEntity target { get; set; } = new DbEntity();

        public CodeHelper Binder { get; set; } = new CodeHelper();
        
        public string ReturnType { get; set; } = string.Empty;

        public string BindModel { get; set; } = string.Empty;
        public bool IsNoModel { get; set; } = false;

        public bool IsAsync { get; set; } = false;

        public BindOption()
        {
            this.Binder = new CodeHelper(this);
        }
    }
}
