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

public record MerchantPaymentRowDto(
    DateTime Date,
    decimal Amount,
    string Method,
    string? ReferenceNo,
    string? Notes);

public record MerchantLedgerRowDto(
    DateTime Date,
    string Type,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal Balance);

public record MerchantLedgerSummaryDto(
    decimal TotalDebit,
    decimal TotalCredit,
    decimal ClosingBalance);

public record MerchantLedgerResultDto(
    decimal OpeningBalance,
    IReadOnlyList<MerchantLedgerRowDto> Rows,
    MerchantLedgerSummaryDto Summary);

public record ProfitBreakdownRowDto(
    string Governorate,
    string Size,
    string PricingCategory,
    int OrdersCount,
    decimal TotalDeliveryFee,
    decimal TotalAgentFees,
    decimal TotalCompanyNetProfit);

public record DeliveryAgentPerformanceRowDto(
    string AgentName,
    int DeliveredCount,
    int ReturnCount,
    int ProblemCount,
    decimal TotalAgentFees,
    decimal TotalCompanyNetProfit,
    int PendingSettlementCount);

public record MerchantStatementWithPaymentsDto(
    MerchantStatementSummaryDto Summary,
    IReadOnlyList<MerchantStatementRowDto> Orders,
    IReadOnlyList<MerchantPaymentRowDto> Payments);
