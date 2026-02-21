using DeliverySaaS.Domain.Identity.Enums;

namespace DeliverySaaS.API.Authorization;

public static class AuthorizationPolicies
{
    public const string CanViewOrders = nameof(CanViewOrders);
    public const string CanTransitionOrders = nameof(CanTransitionOrders);
    public const string CanManageOrderProblems = nameof(CanManageOrderProblems);

    public static string PermissionValue(Permission permission) => permission.ToString();
}
