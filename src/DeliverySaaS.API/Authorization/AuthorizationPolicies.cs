using DeliverySaaS.Domain.Identity.Enums;

namespace DeliverySaaS.API.Authorization;

public static class AuthorizationPolicies
{
    public const string CanViewOrders = nameof(CanViewOrders);
    public const string CanTransitionOrders = nameof(CanTransitionOrders);
    public const string CanManageOrderProblems = nameof(CanManageOrderProblems);
    public const string CanViewFinancialReports = nameof(CanViewFinancialReports);
    public const string CanViewCompanyReports = nameof(CanViewCompanyReports);

    public static string PermissionValue(Permission permission) => permission.ToString();
}
