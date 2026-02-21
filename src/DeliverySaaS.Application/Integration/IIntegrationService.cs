namespace DeliverySaaS.Application.Integration;

public interface IIntegrationService
{
    Task<Guid> CreatePartnerConnectionAsync(string partnerName, string baseUrl, string apiKey, CancellationToken cancellationToken = default);
    Task<Guid> CreateOutboundHandoffAsync(Guid orderId, Guid governorateId, CancellationToken cancellationToken = default);
    Task ReceiveWebhookAsync(string partnerName, string payload, string? signature, string? timestamp, string? nonce, CancellationToken cancellationToken = default);
    Task<int> ProcessOutboxAsync(int take = 50, CancellationToken cancellationToken = default);
}
