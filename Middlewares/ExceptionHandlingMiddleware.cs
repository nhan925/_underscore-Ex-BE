﻿using System.Net;
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
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new
            {
                status = context.Response.StatusCode,
                message = "An unexpected error occurred. Please try again later.",
                details = ex.Message
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
