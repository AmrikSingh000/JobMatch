
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobMatch.Infrastructure
{
    public static class AuditLogger
    {
        public static string GetAuditLogPath(IHostEnvironment env)
        {
            var root = env.ContentRootPath;
            var dir = Path.Combine(root, "App_Data");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "audit.log");
        }

        public static async Task AppendAsync(IHostEnvironment env, string line)
        {
            var path = GetAuditLogPath(env);
            await File.AppendAllTextAsync(path, line + Environment.NewLine);
        }
    }

    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            try
            {
                var user = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity?.Name : "anonymous";
                var path = context.Request.Path.Value ?? "";
                var method = context.Request.Method;
                var ip = context.Connection?.RemoteIpAddress?.ToString() ?? "-";
                var ts = DateTime.UtcNow.ToString("o");

                if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/Jobs", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/Applications", StringComparison.OrdinalIgnoreCase))
                {
                    var line = $"{ts}\t{user}\t{method}\t{path}\t{ip}";
                    await AuditLogger.AppendAsync(_env, line);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log.");
            }
        }
    }
}