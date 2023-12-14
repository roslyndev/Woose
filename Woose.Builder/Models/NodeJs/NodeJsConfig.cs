namespace Woose.Builder
{
    public class NodeJsConfig
    {
        public NodeJsDatabase database { get; set; } = new NodeJsDatabase();

        public string tokenKey { get; set; } = string.Empty;

        public int port { get; set; }

        public string secret { get; set; } = string.Empty;

        public string emailFrom { get; set; } = string.Empty;
    }

    public class NodeJsDatabase
    {
        public string host { get; set; } = string.Empty;

        public int port { get; set; }

        public string user { get; set; } = string.Empty;

        public string password { get; set; } = string.Empty;

        public string database { get; set; } = string.Empty;

        public NodeJsSMTP smtpOptions { get; set; } = new NodeJsSMTP();

        public NodeJsDatabase() 
        { 
        }
    }

    public class NodeJsSMTP
    {
        public string service { get; set; } = string.Empty;

        public string host { get; set; } = string.Empty;

        public int port { get; set; }

        public bool secure { get; set; } = false;

        public NodeJsSMTPAuth auth { get; set; } = new NodeJsSMTPAuth();

        public NodeJsSMTP()
        {
        }
    }

    public class NodeJsSMTPAuth
    {
        public string user { get; set; } = string.Empty;

        public string pass { get; set; } = string.Empty;

        public NodeJsSMTPAuth()
        {
        }
    }
}
