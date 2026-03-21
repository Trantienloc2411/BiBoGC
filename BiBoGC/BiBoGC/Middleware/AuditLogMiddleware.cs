using System.Security.Claims;
using AuthorizationModule.Application.Interfaces;
using AuthorizationModule.Domain.Entities;

namespace BiBoGC.Middleware;

public class AuditLogMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> SkippedPrefixes =
        ["/health", "/_", "/scalar", "/openapi", "/favicon"];

    // Auth endpoints are logged explicitly in AuthService with full context — skip generic logging
    private static readonly HashSet<string> ExplicitlyLoggedPrefixes = ["/api/auth"];

    // Known controller name mappings (plural path segment → singular Pascal-case name)
    private static readonly Dictionary<string, string> ControllerNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["products"]          = "Product",
            ["categories"]        = "Category",
            ["suppliers"]         = "Supplier",
            ["stocktransactions"] = "StockTransaction",
            ["salesorders"]       = "SalesOrder",
            ["invoices"]          = "Invoice",
            ["finance"]           = "Finance",
            ["productvariants"]   = "ProductVariant",
            ["users"]             = "User",
        };

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        await next(context);

        var path = context.Request.Path.Value ?? string.Empty;

        if (SkippedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            return;

        // Auth endpoints already have richer explicit logs — skip duplicate generic entry
        if (ExplicitlyLoggedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            return;

        var statusCode = context.Response.StatusCode;
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;

        // Log unauthenticated 401/403 as security probes; skip everything else unauthenticated
        if (!isAuthenticated && statusCode is not (401 or 403))
            return;

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = context.User.FindFirst(ClaimTypes.Name)?.Value;

        await auditLogService.LogAsync(new AuditLog
        {
            UserId = Guid.TryParse(userIdClaim, out var uid) ? uid : null,
            Username = username,
            Action = ResolveAction(context.Request.Method, path),
            HttpMethod = context.Request.Method,
            Endpoint = path,
            StatusCode = statusCode,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            IsSuccess = statusCode < 400,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Maps HTTP method + path to a semantic action string.
    ///   POST /api/products               → Product.Create
    ///   GET  /api/products               → Product.List
    ///   GET  /api/products/{id}          → Product.Get
    ///   PUT  /api/products/{id}          → Product.Update
    ///   DELETE /api/products/{id}        → Product.Delete
    ///   POST /api/products/{id}/batches  → Product.Batch.Create
    /// </summary>
    private static string ResolveAction(string method, string path)
    {
        var clean = path.Split('?')[0];
        var segments = clean.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Expect at least /api/{controller}
        if (segments.Length < 2) return $"Unknown.{method}";

        var controller = NormalizeName(segments[1]);

        // Sub-resource: /api/{ctrl}/{id}/{subResource}/...
        if (segments.Length >= 4)
        {
            var subResource = NormalizeName(segments[3]);
            return method.ToUpperInvariant() switch
            {
                "GET"    => $"{controller}.{subResource}.List",
                "POST"   => $"{controller}.{subResource}.Create",
                "PUT"    => $"{controller}.{subResource}.Update",
                "PATCH"  => $"{controller}.{subResource}.Update",
                "DELETE" => $"{controller}.{subResource}.Delete",
                _        => $"{controller}.{subResource}.{method}"
            };
        }

        return method.ToUpperInvariant() switch
        {
            "GET"    when segments.Length == 2 => $"{controller}.List",
            "GET"                              => $"{controller}.Get",
            "POST"                             => $"{controller}.Create",
            "PUT"                              => $"{controller}.Update",
            "PATCH"                            => $"{controller}.Update",
            "DELETE"                           => $"{controller}.Delete",
            _                                  => $"{controller}.{method}"
        };
    }

    private static string NormalizeName(string segment)
    {
        if (ControllerNames.TryGetValue(segment, out var mapped)) return mapped;
        return string.IsNullOrEmpty(segment)
            ? "Unknown"
            : char.ToUpperInvariant(segment[0]) + segment[1..];
    }
}

public static class AuditLogMiddlewareExtensions
{
    public static IApplicationBuilder UseAuditLog(this IApplicationBuilder app)
        => app.UseMiddleware<AuditLogMiddleware>();
}
