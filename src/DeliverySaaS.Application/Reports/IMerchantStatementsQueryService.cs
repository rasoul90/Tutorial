namespace DeliverySaaS.Application.Reports;

public interface IMerchantStatementsQueryService
{
    Task<StatementResultDto<MerchantStatementSummaryDto, MerchantStatementRowDto>> GetStatementAsync(
        Guid merchantId,
        DateTime from,
        DateTime to,
        int page,
        int size,
        bool onlyReadyForSettlement,
        CancellationToken cancellationToken = default);
}
