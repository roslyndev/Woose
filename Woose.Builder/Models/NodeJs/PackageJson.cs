using Newtonsoft.Json;

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

        public PackageJsonDependencies dependencies { get; set; } = new PackageJsonDependencies();

        public PackageJsonDevDependencies devDependencies { get; set; } = new PackageJsonDevDependencies();

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

    public class PackageJsonDependencies
    {
        public string axios { get; set; } = string.Empty;
        public string bcryptjs { get; set; } = string.Empty;
        public string cors { get; set; } = string.Empty;
        public string crypto { get; set; } = string.Empty;
        public string express { get; set; } = string.Empty;
        [JsonProperty("express-fileupload")]
        public string expressFileupload { get; set; } = string.Empty;
        public string jsonwebtoken { get; set; } = string.Empty;
        public string moment { get; set; } = string.Empty;
        public string multer { get; set; } = string.Empty;
        public string nodemailer { get; set; } = string.Empty;
        public string nodemon { get; set; } = string.Empty;
        public string sequelize { get; set; } = string.Empty;

        [JsonProperty("firebase-admin")]
        public string firebaseAdmin { get; set; } = string.Empty;

        [JsonProperty("swagger-cli")]
        public string swaggerCli { get; set; } = string.Empty;

        [JsonProperty("swagger-jsdoc")]
        public string swaggerJsdoc { get; set; } = string.Empty;

        [JsonProperty("swagger-ui-express")]
        public string swaggerUiExpress { get; set; } = string.Empty;
        public string tedious { get; set; } = string.Empty;
        public string sharp { get; set; } = string.Empty;

        public string mysql { get; set; } = string.Empty;
        public string mysql2 { get; set; } = string.Empty;

        public string uuid4 { get; set; } = string.Empty;
        public string yamljs { get; set; } = string.Empty;

        public PackageJsonDependencies()
        {
        }
    }

    public class PackageJsonDevDependencies
    {
        [JsonProperty("@types/swagger-ui-express")]
        public string swaggerUiExpress { get; set; } = string.Empty;

        [JsonProperty("@types/yamljs")]
        public string yamljs { get; set; } = string.Empty;

        public PackageJsonDevDependencies()
        {
        }
    }
}