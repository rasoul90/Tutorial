using DeliverySaaS.Application.Auditing;

namespace DeliverySaaS.API.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        await _next(context);

        if (context.Request.Method is not ("POST" or "PUT" or "PATCH" or "DELETE")) return;
        var path = context.Request.Path.Value ?? string.Empty;
        var sensitive = path.Contains("accounting", StringComparison.OrdinalIgnoreCase)
            || path.Contains("payments", StringComparison.OrdinalIgnoreCase)
            || path.Contains("problems", StringComparison.OrdinalIgnoreCase)
            || path.Contains("integration", StringComparison.OrdinalIgnoreCase)
            || path.Contains("transition", StringComparison.OrdinalIgnoreCase);
        if (!sensitive) return;

        var action = path.ToUpperInvariant().Replace('/', '_');
        var summary = $"{context.Request.Method} {path}";
        await auditLogService.WriteAsync(action, "HttpRequest", path, summary, "{}", context.Connection.RemoteIpAddress?.ToString(), context.Request.Headers.UserAgent.ToString());
    }
}
