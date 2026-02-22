namespace DeliverySaaS.Application.Reports;

public interface IDeliveryAgentStatementsQueryService
{
    Task<StatementResultDto<DeliveryAgentStatementSummaryDto, DeliveryAgentStatementRowDto>> GetStatementAsync(
        Guid deliveryAgentId,
        DateTime from,
        DateTime to,
        int page,
        int size,
        bool onlyUnsettled,
        CancellationToken cancellationToken = default);
}
