namespace DeliverySaaS.Application.Reports;

public record BranchFinancialSummaryDto(
    int TotalOrdersDelivered,
    decimal TotalDeliveredValue,
    decimal TotalDeliveryFee,
    decimal TotalDeliveryAgentFees,
    decimal TotalCompanyNetProfit,
    decimal TotalMerchantDue,
    int TotalUnsettledWithDeliveryAgentsCount,
    decimal TotalUnsettledWithDeliveryAgentsAmount,
    int TotalUnsettledWithMerchantsCount,
    decimal TotalUnsettledWithMerchantsAmount);

public record MerchantStatementRowDto(
    DateTime Date,
    string OrderNo,
    string CustomerName,
    string Governorate,
    string Size,
    decimal DeliveredPriceWithDelivery,
    decimal DeliveryFeeApplied,
    decimal MerchantDueAmount,
    bool IsDeliveryAgentSettled,
    bool IsMerchantSettled,
    bool HasReturn,
    string? Notes);

public record MerchantStatementSummaryDto(
    decimal TotalDeliveredValue,
    decimal TotalDeliveryFee,
    decimal TotalMerchantDue,
    decimal TotalSettledToMerchant,
    decimal TotalPendingToMerchant,
    decimal TotalNotReady);

public record DeliveryAgentStatementRowDto(
    DateTime Date,
    string OrderNo,
    decimal DeliveredPriceWithDelivery,
    decimal DeliveryFeeApplied,
    decimal DeliveryAgentFeeApplied,
    decimal CompanyNetDeliveryProfit,
    bool IsDeliveryAgentSettled,
    bool HasReturn);

public record DeliveryAgentStatementSummaryDto(
    decimal TotalDeliveredValue,
    decimal TotalDeliveryFee,
    decimal TotalAgentFees,
    decimal TotalCompanyNetProfit,
    int TotalPendingSettlementCount,
    decimal TotalPendingSettlementAmount);

public record StatementResultDto<TSummary, TRow>(TSummary Summary, IReadOnlyList<TRow> Rows);
