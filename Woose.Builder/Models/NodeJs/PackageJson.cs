namespace Woose.Builder
{
    public class PackageJson
    {
        public string name { get; set; } = string.Empty;

        public string version { get; set; } = string.Empty;

        public string description { get; set; } = string.Empty;

        public string main { get; set; } = string.Empty;

        public PackageJsonScripts scripts { get; set; } = new PackageJsonScripts();

        public PackageJsonRepository repository { get; set; } = new PackageJsonRepository();

        public List<string> keywords { get; set; } = new List<string>();

        public string author { get; set; } = string.Empty;

        public string license { get; set; } = string.Empty;

        public PackageJsonBugs bugs { get; set; } = new PackageJsonBugs();

        public string homepage { get; set; } = string.Empty;

        public dynamic dependencies { get; set; }

        public dynamic devDependencies { get; set; }

        public PackageJson()
        {
        }
    }

    public class PackageJsonScripts
    {
        public string start { get; set; } = string.Empty;

        public string test { get; set; } = string.Empty;
    }

    public class PackageJsonRepository
    {
        public string type { get; set; } = string.Empty;

        public string url { get; set; } = string.Empty;

        public PackageJsonRepository()
        {
        }
    }

    public class PackageJsonBugs
    {
        public string url { get; set; } = string.Empty;

        public PackageJsonBugs()
        {
        }
    }
}