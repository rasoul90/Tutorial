using System.Globalization;
using System.Text.Json;
using DeliverySaaS.Application.Auditing;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Application.Notifications;
using DeliverySaaS.Domain.Integration.Entities;

namespace DeliverySaaS.Application.Integration;

public class IntegrationService : IIntegrationService
{
    private readonly IIntegrationRepository _integrationRepository;
    private readonly IRequestContext _requestContext;
    private readonly INotificationService? _notificationService;
    private readonly IAuditLogService? _auditLogService;
    private readonly IHmacSignatureService _hmacSignatureService;
    private readonly IReplayProtectionService _replayProtectionService;
    private readonly IReferenceDataCacheService _referenceDataCacheService;

    public IntegrationService(
        IIntegrationRepository integrationRepository,
        IRequestContext requestContext,
        IHmacSignatureService hmacSignatureService,
        IReplayProtectionService replayProtectionService,
        IReferenceDataCacheService referenceDataCacheService,
        INotificationService? notificationService = null,
        IAuditLogService? auditLogService = null)
    {
        _integrationRepository = integrationRepository;
        _requestContext = requestContext;
        _hmacSignatureService = hmacSignatureService;
        _replayProtectionService = replayProtectionService;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _referenceDataCacheService = referenceDataCacheService;
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
        var governorateExists = await _referenceDataCacheService.GovernorateExistsAsync(governorateId, cancellationToken);
        if (!governorateExists)
        {
            throw new InvalidOperationException("Governorate not found.");
        }

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

    public async Task ReceiveWebhookAsync(
        string partnerName,
        string payload,
        string? signature,
        string? timestamp,
        string? nonce,
        CancellationToken cancellationToken = default)
    {
        var partner = await _integrationRepository.GetPartnerConnectionByNameAsync(partnerName, cancellationToken)
            ?? throw new InvalidOperationException("Partner connection not found.");

        if (!TryParseTimestamp(timestamp, out var parsedTimestamp))
        {
            throw new InvalidOperationException("Missing or invalid webhook timestamp.");
        }

        if (string.IsNullOrWhiteSpace(nonce))
        {
            throw new InvalidOperationException("Missing webhook nonce.");
        }

        if (DateTimeOffset.UtcNow - parsedTimestamp > TimeSpan.FromMinutes(5) || parsedTimestamp - DateTimeOffset.UtcNow > TimeSpan.FromMinutes(1))
        {
            throw new InvalidOperationException("Webhook timestamp is outside accepted window.");
        }

        var replayKey = $"webhook:{partnerName}:{nonce}:{timestamp}";
        if (_replayProtectionService.IsReplay(replayKey, TimeSpan.FromMinutes(10)))
        {
            throw new InvalidOperationException("Replay attack detected.");
        }

        var canonicalPayload = $"{timestamp}.{nonce}.{payload}";
        if (!_hmacSignatureService.ValidateSignature(canonicalPayload, partner.ApiKey, signature))
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
                _ = root.GetProperty("handoffId").GetGuid();

                msg.ProcessedAt = DateTime.UtcNow;
                msg.Error = null;
                processed++;
            }
            catch (Exception ex)
            {
                msg.Error = ex.Message;
                if (_notificationService != null)
                {
                    await _notificationService.CreateAsync(null, "CompanyAdmin", "فشل تكامل", "فشل إرسال رسالة تكامل صادرة", Domain.Notifications.Enums.NotificationType.Integration, "Handoff", msg.Id, cancellationToken);
                }
                if (_auditLogService != null)
                {
                    await _auditLogService.WriteAsync("INTEGRATION_OUTBOUND_FAILED", "OutboxMessage", msg.Id.ToString(), "فشل إرسال تكامل", "{}", cancellationToken: cancellationToken);
                }
            }
        }

        await _integrationRepository.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private Guid RequiredBranchId() => _requestContext.BranchId ?? throw new InvalidOperationException("BranchId is required.");

    private static bool TryParseTimestamp(string? timestamp, out DateTimeOffset parsed)
    {
        return DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out parsed);
    }
}
