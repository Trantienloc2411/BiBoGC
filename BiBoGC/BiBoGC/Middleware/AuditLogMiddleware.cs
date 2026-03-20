using System.Security.Claims;
using AuthorizationModule.Application.Interfaces;
using AuthorizationModule.Domain.Entities;

namespace BiBoGC.Middleware;

public class AuditLogMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> SkippedPrefixes = ["/health", "/_", "/scalar", "/openapi", "/favicon"];

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        await next(context);

        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var path = context.Request.Path.Value ?? string.Empty;
        if (SkippedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            return;

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = context.User.FindFirst(ClaimTypes.Name)?.Value;

        var log = new AuditLog
        {
            UserId = Guid.TryParse(userIdClaim, out var uid) ? uid : null,
            Username = username,
            Action = "API_Access",
            HttpMethod = context.Request.Method,
            Endpoint = path,
            StatusCode = context.Response.StatusCode,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            IsSuccess = context.Response.StatusCode < 400,
            Timestamp = DateTime.UtcNow
        };

        await auditLogService.LogAsync(log);
    }
}

public static class AuditLogMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditLog(this IApplicationBuilder app)
        => app.UseMiddleware<AuditLogMiddleware>();
}
