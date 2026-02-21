using DeliverySaaS.Domain.Integration.Entities;

namespace DeliverySaaS.Application.Common.Interfaces;

public interface IIntegrationRepository
{
    Task AddPartnerConnectionAsync(PartnerConnection entity, CancellationToken cancellationToken = default);
    Task<PartnerConnection?> GetPartnerConnectionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PartnerConnection?> GetPartnerConnectionByNameAsync(string partnerName, CancellationToken cancellationToken = default);
    Task<RoutingRule?> GetRoutingRuleByGovernorateAsync(Guid governorateId, CancellationToken cancellationToken = default);
    Task AddOrderHandoffAsync(OrderHandoff handoff, CancellationToken cancellationToken = default);
    Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task AddInboxMessageAsync(InboxMessage message, CancellationToken cancellationToken = default);
    Task<List<OutboxMessage>> GetPendingOutboxAsync(int take, CancellationToken cancellationToken = default);
    Task<bool> InboxMessageExistsAsync(string messageType, string payload, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
