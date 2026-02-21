using DeliverySaaS.Domain.Identity.Enums;

namespace DeliverySaaS.API.Authorization;

public static class RolePermissionMap
{
    public static readonly IReadOnlyDictionary<string, Permission[]> Map =
        new Dictionary<string, Permission[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Courier"] = [Permission.OrdersView, Permission.OrdersTransition],
            ["Dispatcher"] = [Permission.OrdersView, Permission.OrdersTransition, Permission.OrderProblemsManage],
            ["CompanyAdmin"] = [Permission.OrdersView, Permission.OrdersTransition, Permission.OrderProblemsManage, Permission.BranchManage, Permission.TenantManage],
            ["SaaSAdmin"] = [Permission.OrdersView, Permission.OrdersTransition, Permission.OrderProblemsManage, Permission.BranchManage, Permission.TenantManage, Permission.SaaSManage]
        };
}
