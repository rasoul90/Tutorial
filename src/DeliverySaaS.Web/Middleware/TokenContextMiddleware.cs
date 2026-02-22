namespace DeliverySaaS.Web.Middleware;

public class TokenContextMiddleware
{
    private readonly RequestDelegate _next;

    public TokenContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            token = context.Request.Cookies["access_token"];
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            context.Items["AccessToken"] = token;
        }

        await _next(context);
    }
}
