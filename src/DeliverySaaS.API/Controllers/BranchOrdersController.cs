using DeliverySaaS.API.Authorization;
using DeliverySaaS.Application.Common.Models;
using DeliverySaaS.Application.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/branch/orders")]
[Authorize(Policy = AuthorizationPolicies.CanViewFinancialReports)]
public class BranchOrdersController : ControllerBase
{
    private readonly IOrderSearchQueryService _orderSearchQueryService;

    public BranchOrdersController(IOrderSearchQueryService orderSearchQueryService)
    {
        _orderSearchQueryService = orderSearchQueryService;
    }

    [HttpGet("search")]
    public Task<PagedResult<OrderSearchRowDto>> Search([FromQuery] bool includeArchive = false, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken cancellationToken = default)
        => _orderSearchQueryService.SearchAsync(includeArchive, page, size, cancellationToken);
}
