using DeliverySaaS.Domain.Operations.Enums;

namespace DeliverySaaS.Application.Orders;

public interface IOrderProblemService
{
    Task<Guid> CreateProblemAsync(Guid orderId, Guid problemCatalogId, string? notes, CancellationToken cancellationToken = default);
    Task ResolveProblemAsync(Guid problemId, DynamicResolutionType resolutionType, string? phone, decimal? amountToCollect, string? address, string? note, CancellationToken cancellationToken = default);
}
