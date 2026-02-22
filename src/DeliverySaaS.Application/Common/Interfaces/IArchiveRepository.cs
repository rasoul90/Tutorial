using DeliverySaaS.Domain.Operations.ArchiveEntities;
using DeliverySaaS.Domain.Operations.Entities;

namespace DeliverySaaS.Application.Common.Interfaces;

public interface IArchiveRepository
{
    Task<List<Order>> GetOrdersEligibleForArchiveAsync(DateTime cutoff, CancellationToken cancellationToken = default);
    Task<List<OrderEvent>> GetOrderEventsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken cancellationToken = default);
    Task<List<OrderProblem>> GetOrderProblemsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken cancellationToken = default);
    Task AddOrderArchivesAsync(IEnumerable<OrderArchive> entities, CancellationToken cancellationToken = default);
    Task AddOrderEventArchivesAsync(IEnumerable<OrderEventArchive> entities, CancellationToken cancellationToken = default);
    Task AddOrderProblemArchivesAsync(IEnumerable<OrderProblemArchive> entities, CancellationToken cancellationToken = default);
    Task RemoveOrdersAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default);
    Task RemoveOrderEventsAsync(IEnumerable<OrderEvent> events, CancellationToken cancellationToken = default);
    Task RemoveOrderProblemsAsync(IEnumerable<OrderProblem> problems, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
