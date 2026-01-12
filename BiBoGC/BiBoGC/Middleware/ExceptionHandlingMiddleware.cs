using FluentValidation;
using System.Net;
using System.Text.Json;

namespace BiBoGC.Middleware;

/// <summary>
/// Global exception handling middleware
/// Catches all exceptions and returns appropriate HTTP responses
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Dữ liệu không hợp lệ",
                validationEx.Errors.Select(e => e.ErrorMessage).ToArray()
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                argEx.Message,
                Array.Empty<string>()
            ),
            InvalidOperationException invOpEx => (
                HttpStatusCode.BadRequest,
                invOpEx.Message,
                Array.Empty<string>()
            ),
            KeyNotFoundException keyNotFoundEx => (
                HttpStatusCode.NotFound,
                keyNotFoundEx.Message,
                Array.Empty<string>()
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Đã xảy ra lỗi server. Vui lòng thử lại sau.",
                Array.Empty<string>()
            )
        };

        response.StatusCode = (int)statusCode;

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title = message,
            status = (int)statusCode,
            errors = errors.Length > 0 ? errors : null,
            traceId = context.TraceIdentifier
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
