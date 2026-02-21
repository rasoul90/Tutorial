using System.Text.Json;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Integration.Entities;

namespace DeliverySaaS.Application.Integration;

public class IntegrationService : IIntegrationService
{
    private readonly IIntegrationRepository _integrationRepository;
    private readonly IRequestContext _requestContext;
    private readonly IHmacSignatureService _hmacSignatureService;

    public IntegrationService(IIntegrationRepository integrationRepository, IRequestContext requestContext, IHmacSignatureService hmacSignatureService)
    {
        _integrationRepository = integrationRepository;
        _requestContext = requestContext;
        _hmacSignatureService = hmacSignatureService;
    }

    public async Task<Guid> CreatePartnerConnectionAsync(string partnerName, string baseUrl, string apiKey, CancellationToken cancellationToken = default)
    {
        var entity = new PartnerConnection
        {
            PartnerName = partnerName,
            BaseUrl = baseUrl,
            ApiKey = apiKey,
            BranchId = RequiredBranchId(),
            IsActive = true
        };

        await _integrationRepository.AddPartnerConnectionAsync(entity, cancellationToken);

        await _integrationRepository.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<Guid> CreateOutboundHandoffAsync(Guid orderId, Guid governorateId, CancellationToken cancellationToken = default)
    {
        var rule = await _integrationRepository.GetRoutingRuleByGovernorateAsync(governorateId, cancellationToken)
            ?? throw new InvalidOperationException("No active routing rule for this governorate.");

        var handoff = new OrderHandoff
        {
            OrderId = orderId,
            PartnerConnectionId = rule.PartnerConnectionId,
            Status = "Pending",
            BranchId = RequiredBranchId()
        };

        var payload = JsonSerializer.Serialize(new
        {
            orderId,
            governorateId,
            handoffId = handoff.Id,
            createdAt = DateTime.UtcNow
        });

        var outbox = new OutboxMessage
        {
            Type = "OrderHandoffOutbound",
            Payload = payload
        };

        await _integrationRepository.AddOrderHandoffAsync(handoff, cancellationToken);
        await _integrationRepository.AddOutboxMessageAsync(outbox, cancellationToken);
        await _integrationRepository.SaveChangesAsync(cancellationToken);

        return handoff.Id;
    }

    public async Task ReceiveWebhookAsync(string partnerName, string payload, string? signature, CancellationToken cancellationToken = default)
    {
        var partner = await _integrationRepository.GetPartnerConnectionByNameAsync(partnerName, cancellationToken)
            ?? throw new InvalidOperationException("Partner connection not found.");

        if (!_hmacSignatureService.ValidateSignature(payload, partner.ApiKey, signature))
        {
            throw new InvalidOperationException("Invalid HMAC signature.");
        }

        if (await _integrationRepository.InboxMessageExistsAsync("PartnerWebhook", payload, cancellationToken))
        {
            return;
        }

        var inbox = new InboxMessage
        {
            Type = "PartnerWebhook",
            Payload = payload,
            ReceivedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        await _integrationRepository.AddInboxMessageAsync(inbox, cancellationToken);
        await _integrationRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ProcessOutboxAsync(int take = 50, CancellationToken cancellationToken = default)
    {
        var pending = await _integrationRepository.GetPendingOutboxAsync(take, cancellationToken);
        var processed = 0;

        foreach (var msg in pending)
        {
            try
            {
                using var doc = JsonDocument.Parse(msg.Payload);
                var root = doc.RootElement;
                var handoffId = root.GetProperty("handoffId").GetGuid();

                // Placeholder dispatch: in real worker this sends HTTP with HMAC and retries.
                msg.ProcessedAt = DateTime.UtcNow;
                msg.Error = null;
                processed++;
            }
            catch (Exception ex)
            {
                msg.Error = ex.Message;
            }
        }

        await _integrationRepository.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private Guid RequiredBranchId() => _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");
}
