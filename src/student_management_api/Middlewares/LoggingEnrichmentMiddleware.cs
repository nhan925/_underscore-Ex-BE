using Serilog.Context;
using System.Security.Claims;

namespace student_management_api.Middlewares;

public class LoggingEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Extract UserId from Claims
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
        var correlationId = context.TraceIdentifier;
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("Method", context.Request.Method))
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("ClientIp", clientIp))
        using (LogContext.PushProperty("UserAgent", userAgent))
        {
            await _next(context);
        }
    }
}


