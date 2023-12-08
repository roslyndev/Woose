using System;

namespace Woose.Data
{
    public class AppSettings
    {
        private static readonly Lazy<AppSettings> app = new Lazy<AppSettings>(() => new AppSettings());
        public static AppSettings Current { get { return app.Value; } }

        public string AppName { get; set; } = string.Empty;
        public string AllowedHosts { get; set; } = string.Empty;

        public string ServerToken { get; set; } = string.Empty;

        public AppSettingConfig Config { get; set; } = new AppSettingConfig();

        public AppDatabase Database { get; set; } = new AppDatabase();

        public AppSettings()
        {
        }
    }

    public class AppSettingConfig
    {
        public string AppID { get; set; } = string.Empty;
        public string CookieVar { get; set; } = string.Empty;

        public AppSettingConfig()
        {
        }
    }

    public class AppDatabase
    {
        public string ConnectionString { get; set; } = string.Empty;

        public AppDatabase()
        {
        }
    }
}
