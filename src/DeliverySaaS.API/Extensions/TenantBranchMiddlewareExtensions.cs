using DeliverySaaS.API.Middleware;

namespace DeliverySaaS.API.Extensions;

public static class TenantBranchMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantBranchExtraction(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantBranchExtractionMiddleware>();
    }
}
