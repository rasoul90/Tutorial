using DeliverySaaS.Application.Integration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/integration")]
[Authorize]
public class IntegrationController : ControllerBase
{
    private readonly IIntegrationService _integrationService;

    public IntegrationController(IIntegrationService integrationService)
    {
        _integrationService = integrationService;
    }

    [HttpPost("partner-connections")]
    public async Task<IActionResult> CreatePartnerConnection([FromBody] CreatePartnerConnectionRequest request, CancellationToken cancellationToken)
    {
        var id = await _integrationService.CreatePartnerConnectionAsync(request.PartnerName, request.BaseUrl, request.ApiKey, cancellationToken);
        return Ok(new { partnerConnectionId = id });
    }

    [HttpPost("orders/{orderId:guid}/outbound-handoff")]
    public async Task<IActionResult> CreateOutboundHandoff([FromRoute] Guid orderId, [FromBody] OutboundHandoffRequest request, CancellationToken cancellationToken)
    {
        var handoffId = await _integrationService.CreateOutboundHandoffAsync(orderId, request.GovernorateId, cancellationToken);
        return Ok(new { handoffId });
    }

    [AllowAnonymous]
    [HttpPost("webhooks/{partnerName}")]
    public async Task<IActionResult> ReceiveWebhook([FromRoute] string partnerName, [FromHeader(Name = "X-Signature")] string? signature, [FromBody] System.Text.Json.JsonElement payload, CancellationToken cancellationToken)
    {
        var rawPayload = payload.GetRawText();
        await _integrationService.ReceiveWebhookAsync(partnerName, rawPayload, signature, cancellationToken);
        return Ok(new { message = "Webhook received." });
    }

    [HttpPost("outbox/process")]
    public async Task<IActionResult> ProcessOutbox([FromBody] ProcessOutboxRequest request, CancellationToken cancellationToken)
    {
        var processed = await _integrationService.ProcessOutboxAsync(request.Take <= 0 ? 50 : request.Take, cancellationToken);
        return Ok(new { processed });
    }
}

public class CreatePartnerConnectionRequest
{
    public string PartnerName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class OutboundHandoffRequest
{
    public Guid GovernorateId { get; set; }
}

public class ProcessOutboxRequest
{
    public int Take { get; set; } = 50;
}
