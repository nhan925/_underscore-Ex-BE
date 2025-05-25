using Microsoft.AspNetCore.Http.HttpResults;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using System.Net;
using System.Text.Json;

namespace student_management_api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred in {Path}", context.Request.Path);
            context.Response.ContentType = "application/json";

            var message = ex.Message;
            string? details = null;

            if (ex is NotFoundException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            else if (ex is ForbiddenException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred. Please try again later.";
                details = ex.Message;
            }

            var errorResponse = new ErrorResponse<string>(
                context.Response.StatusCode,
                message,
                details
            );

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
