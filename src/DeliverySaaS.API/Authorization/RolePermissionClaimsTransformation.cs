using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace DeliverySaaS.API.Authorization;

public class RolePermissionClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        var roleClaims = identity.Claims
            .Where(x => x.Type == ClaimTypes.Role || x.Type == "role")
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var role in roleClaims)
        {
            if (!RolePermissionMap.Map.TryGetValue(role, out var permissions))
            {
                continue;
            }

            foreach (var permission in permissions)
            {
                if (!identity.HasClaim("permission", permission.ToString()))
                {
                    identity.AddClaim(new Claim("permission", permission.ToString()));
                }
            }
        }

        return Task.FromResult(principal);
    }
}
