using System.Security.Claims;
using DeliverySaaS.API.Security;

namespace DeliverySaaS.API.Middleware;

public class TenantBranchExtractionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantBranchExtractionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RequestContext requestContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
            var branchClaim = context.User.FindFirst("branch_id")?.Value;

            if (!Guid.TryParse(tenantClaim, out var tenantId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or missing tenant_id claim.");
                return;
            }

            requestContext.TenantId = tenantId;

            if (!string.IsNullOrWhiteSpace(branchClaim) && Guid.TryParse(branchClaim, out var branchId))
            {
                requestContext.BranchId = branchId;
            }
        }

        await _next(context);
    }
}
