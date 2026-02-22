using Microsoft.AspNetCore.Authorization;

namespace DeliverySaaS.API.Authorization;

public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"PERMISSION:{permission}";
    }
}
