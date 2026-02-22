namespace DeliverySaaS.Application.Reports;

public interface IProfitReportsQueryService
{
    Task<IReadOnlyList<ProfitBreakdownRowDto>> GetProfitBreakdownAsync(DateTime from, DateTime to, string groupBy, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryAgentPerformanceRowDto>> GetDeliveryAgentPerformanceAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
