using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DeliverySaaS.API.Controllers.Mobile;

[ApiController]
[Route("api/mobile/pickup")]
[Authorize]
[EnableRateLimiting("MobilePolicy")]
public class PickupMobileController : ControllerBase
{
    private readonly IOrderMobileService _orderMobileService;
    private readonly IOrderService _orderService;

    public PickupMobileController(IOrderMobileService orderMobileService, IOrderService orderService)
    {
        _orderMobileService = orderMobileService;
        _orderService = orderService;
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> TaskList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var list = await _orderMobileService.GetPickupTasksAsync(pageNumber, pageSize, cancellationToken);
        return Ok(list);
    }

    [HttpPost("qr-scan-transition")]
    public async Task<IActionResult> QrScanTransition([FromBody] PickupQrTransitionRequest request, CancellationToken cancellationToken)
    {
        await _orderService.TransitionAsync(request.OrderId, request.ToState, false, cancellationToken);
        return Ok(new { message = "Transition completed." });
    }
}

public class PickupQrTransitionRequest
{
    public Guid OrderId { get; set; }
    public OperationalState ToState { get; set; }
}
