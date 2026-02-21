using DeliverySaaS.Application.Accounting;
using DeliverySaaS.Application.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DeliverySaaS.API.Controllers.Mobile;

[ApiController]
[Route("api/mobile/merchant")]
[Authorize]
[EnableRateLimiting("MobilePolicy")]
public class MerchantMobileController : ControllerBase
{
    private readonly IOrderMobileService _orderMobileService;
    private readonly IAccountingService _accountingService;

    public MerchantMobileController(IOrderMobileService orderMobileService, IAccountingService accountingService)
    {
        _orderMobileService = orderMobileService;
        _accountingService = accountingService;
    }

    [HttpGet("dashboard/{merchantId:guid}")]
    public async Task<IActionResult> Dashboard([FromRoute] Guid merchantId, CancellationToken cancellationToken)
    {
        var data = await _orderMobileService.GetMerchantDashboardAsync(merchantId, cancellationToken);
        return Ok(data);
    }

    [HttpPost("orders/reserved-qr")]
    public async Task<IActionResult> CreateOrderByReservedQr([FromBody] CreateOrderByReservedQrRequest request, CancellationToken cancellationToken)
    {
        var orderId = await _orderMobileService.CreateOrderByReservedQrAsync(request, cancellationToken);
        return Ok(new { orderId });
    }

    [HttpPost("settlement-requests")]
    public async Task<IActionResult> SettlementRequest([FromBody] MerchantSettlementRequestMobileRequest request, CancellationToken cancellationToken)
    {
        var id = await _accountingService.CreateMerchantSettlementRequestAsync(request.MerchantId, request.Amount, cancellationToken);
        return Ok(new { settlementRequestId = id });
    }
}

public class MerchantSettlementRequestMobileRequest
{
    public Guid MerchantId { get; set; }
    public decimal Amount { get; set; }
}
