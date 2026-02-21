using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("{id:guid}/transition")]
    public async Task<IActionResult> Transition([FromRoute] Guid id, [FromBody] TransitionOrderRequest request, CancellationToken cancellationToken)
    {
        await _orderService.TransitionAsync(id, request.ToState, cancellationToken);
        return Ok(new { message = "Order transitioned successfully." });
    }
}

public class TransitionOrderRequest
{
    public OperationalState ToState { get; set; }
}
