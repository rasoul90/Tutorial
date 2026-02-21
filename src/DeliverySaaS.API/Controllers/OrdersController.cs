using DeliverySaaS.API.Authorization;
using DeliverySaaS.Application.Orders;
using DeliverySaaS.Domain.Operations.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Policy = AuthorizationPolicies.CanViewOrders)]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderProblemService _orderProblemService;

    public OrdersController(IOrderService orderService, IOrderProblemService orderProblemService)
    {
        _orderService = orderService;
        _orderProblemService = orderProblemService;
    }

    [HttpPost("{id:guid}/transition")]
    [Authorize(Policy = AuthorizationPolicies.CanTransitionOrders)]
    public async Task<IActionResult> Transition([FromRoute] Guid id, [FromBody] TransitionOrderRequest request, CancellationToken cancellationToken)
    {
        await _orderService.TransitionAsync(id, request.ToState, cancellationToken);
        return Ok(new { message = "Order transitioned successfully." });
    }

    [HttpPost("{id:guid}/problems")]
    [Authorize(Policy = AuthorizationPolicies.CanManageOrderProblems)]
    public async Task<IActionResult> CreateProblem([FromRoute] Guid id, [FromBody] CreateOrderProblemRequest request, CancellationToken cancellationToken)
    {
        var problemId = await _orderProblemService.CreateProblemAsync(id, request.ProblemCatalogId, request.Notes, cancellationToken);
        return Ok(new { problemId });
    }

    [HttpPost("problems/{problemId:guid}/resolve")]
    [Authorize(Policy = AuthorizationPolicies.CanManageOrderProblems)]
    public async Task<IActionResult> ResolveProblem([FromRoute] Guid problemId, [FromBody] ResolveOrderProblemRequest request, CancellationToken cancellationToken)
    {
        await _orderProblemService.ResolveProblemAsync(problemId, request.ResolutionType, request.Phone, request.AmountToCollect, request.Address, request.Note, cancellationToken);
        return Ok(new { message = "Problem resolved successfully." });
    }
}

public class TransitionOrderRequest
{
    public OperationalState ToState { get; set; }
}

public class CreateOrderProblemRequest
{
    public Guid ProblemCatalogId { get; set; }
    public string? Notes { get; set; }
}

public class ResolveOrderProblemRequest
{
    public DynamicResolutionType ResolutionType { get; set; }
    public string? Phone { get; set; }
    public decimal? AmountToCollect { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
}
