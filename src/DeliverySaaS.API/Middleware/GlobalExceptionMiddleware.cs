using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception for request {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, ex);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        var statusCode = ex switch
        {
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == 500 ? "حدث خطأ غير متوقع" : "تعذر تنفيذ الطلب",
            Detail = statusCode == 500 ? null : ex.Message,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
