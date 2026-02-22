namespace DeliverySaaS.API.Authorization;

public static class AuthorizationPolicies
{
    public const string CanViewOrders = nameof(CanViewOrders);
    public const string CanTransitionOrders = nameof(CanTransitionOrders);
    public const string CanManageOrderProblems = nameof(CanManageOrderProblems);
    public const string CanViewFinancialReports = nameof(CanViewFinancialReports);
    public const string CanViewCompanyReports = nameof(CanViewCompanyReports);
    public const string CanManageMerchantPayments = nameof(CanManageMerchantPayments);

    public const string OrdersViewPermission = "ORDER.VIEW";
    public const string OrdersTransitionPermission = "ORDER.TRANSITION";
    public const string OrderProblemsManagePermission = "ORDER_PROBLEM.MANAGE";
    public const string FinReportsViewPermission = "REPORT.BRANCH.VIEW";
    public const string TenantReportsViewPermission = "REPORT.COMPANY.VIEW";
    public const string MerchantPaymentsManagePermission = "PAYMENT.MANAGE";
}
