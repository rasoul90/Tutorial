namespace DeliverySaaS.Application.Reports;

public interface IFinancialReportsQueryService
{
    Task<BranchFinancialSummaryDto> GetBranchFinancialSummaryAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<BranchFinancialSummaryDto> GetCompanyFinancialSummaryAsync(DateTime from, DateTime to, Guid? branchId = null, CancellationToken cancellationToken = default);
}
