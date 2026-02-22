using Microsoft.AspNetCore.Authentication;

namespace DeliverySaaS.API.Authorization;

public class RolePermissionClaimsTransformation : IClaimsTransformation
{
    public Task<System.Security.Claims.ClaimsPrincipal> TransformAsync(System.Security.Claims.ClaimsPrincipal principal)
        => Task.FromResult(principal);
}
