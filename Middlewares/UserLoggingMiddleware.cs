using Serilog.Context;
using System.Security.Claims;

namespace student_management_api.Middlewares;

public class UserLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UserLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

        using (LogContext.PushProperty("UserId", userId))
        {
            await _next(context);
        }
    }
}
