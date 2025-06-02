using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using student_management_api.Exceptions;
using student_management_api.Helpers;
using System.Net;
using System.Text.Json;
using student_management_api.Resources;

namespace student_management_api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IStringLocalizer<Messages> _localizer;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IStringLocalizer<Messages> localizer)
    {
        _next = next;
        _logger = logger;
        _localizer = localizer;
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
                message = _localizer["an_unexpected_error_occurred_Please_try_again_later"];
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
