using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.Xml;
using Woose.Data;

namespace Woose.API
{
    public class BaseController : Controller
    {
        protected readonly IContext context;
        protected ICryptoHandler crypto;
        protected IConfiguration config;
        
        public string access_token { get; set; } = string.Empty;

        public string request_server_token { get; set; } = string.Empty;

        public bool IsLogin { get; set; } = false;

        public BaseController(IContext _context, ICryptoHandler _crypto, IConfiguration configuration)
        {
            this.context = _context;
            this.crypto = _crypto;
            this.config = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (this.config != null)
            {
                AppSettings.Current.AppName = this.config.GetSection("AppName").Value ?? string.Empty;
                AppSettings.Current.ServerToken = this.config.GetSection("ServerToken").Value ?? string.Empty;
                AppSettings.Current.Config.AppID = this.config.GetSection("Config").GetSection("AppID").Value ?? string.Empty;
                AppSettings.Current.Config.CookieVar = this.config.GetSection("Config").GetSection("CookieVar").Value ?? string.Empty;
                AppSettings.Current.Database.ConnectionString = this.config.GetSection("Database").GetSection("ConnectionString").Value ?? string.Empty;
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

        protected virtual User? GetAccessToken()
        {
            User? result = null;

            if (HttpContext.Request.Headers.TryGetValue("servertoken", out var servertoken))
            {
                this.request_server_token = servertoken.ToString();
            }

            if (HttpContext.Request.Headers.TryGetValue("access_token", out var authHeader))
            {
                this.access_token = authHeader.ToString().Replace("Bearer ", "");
                if (!string.IsNullOrWhiteSpace(this.access_token))
                {
                    result = crypto.GetUserFromToken(this.access_token);
                    if (result != null && !string.IsNullOrWhiteSpace(result.Id))
                    {
                        this.IsLogin = true;
                    }
                    else
                    {
                        result = new User();
                        result.ServerToken = this.request_server_token;
                    }
                }
                else
                {
                    result = new User();
                    result.ServerToken = this.request_server_token;
                }
            }
            else
            {
                result = new User();
                result.ServerToken = this.request_server_token;
            }
            
            return result;
        }
    }
}
