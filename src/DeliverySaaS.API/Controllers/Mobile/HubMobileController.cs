using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers.Mobile;

[ApiController]
[Route("api/mobile/hub")]
[Authorize]
public class HubMobileController : ControllerBase
{
    private readonly IOrderService _orderService;

    public HubMobileController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("receive-from-pickup")]
    public async Task<IActionResult> ReceiveFromPickup([FromBody] HubTransitionRequest request, CancellationToken cancellationToken)
    {
        await _orderService.TransitionAsync(request.OrderId, OperationalState.InSortingHub, cancellationToken);
        return Ok(new { message = "Received from pickup." });
    }

    [HttpPost("handover-to-delivery")]
    public async Task<IActionResult> HandoverToDelivery([FromBody] HubTransitionRequest request, CancellationToken cancellationToken)
    {
        await _orderService.TransitionAsync(request.OrderId, OperationalState.InDeliveryAgent, cancellationToken);
        return Ok(new { message = "Handed over to delivery." });
    }

    [HttpPost("receive-returns")]
    public async Task<IActionResult> ReceiveReturns([FromBody] HubTransitionRequest request, CancellationToken cancellationToken)
    {
        await _orderService.TransitionAsync(request.OrderId, OperationalState.ReturnSortingHub, cancellationToken);
        return Ok(new { message = "Returns received at hub." });
    }
}

public class HubTransitionRequest
{
    public Guid OrderId { get; set; }
}
