using DeliverySaaS.API.Authorization;
using DeliverySaaS.Application.Payments;
using DeliverySaaS.Domain.Accounting.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/branch/merchant-payments")]
[Authorize(Policy = AuthorizationPolicies.CanManageMerchantPayments)]
public class PaymentsController : ControllerBase
{
    private readonly IMerchantPaymentsService _merchantPaymentsService;

    public PaymentsController(IMerchantPaymentsService merchantPaymentsService)
    {
        _merchantPaymentsService = merchantPaymentsService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMerchantPaymentApiRequest request, CancellationToken cancellationToken)
    {
        var result = await _merchantPaymentsService.CreatePaymentAsync(new CreateMerchantPaymentRequest
        {
            MerchantId = request.MerchantId,
            PaymentDate = request.PaymentDate,
            Amount = request.Amount,
            Method = request.Method,
            ReferenceNo = request.ReferenceNo,
            Notes = request.Notes,
            CreatedByUserId = request.CreatedByUserId,
            AutoAllocateFifo = request.AutoAllocateFifo,
            ManualAllocations = request.ManualAllocations.Select(x => new ManualAllocationRequestDto(x.OrderId, x.Amount)).ToList()
        }, cancellationToken);

        return Ok(result);
    }
}

public class CreateMerchantPaymentApiRequest
{
    public Guid MerchantId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedByUserId { get; set; }
    public bool AutoAllocateFifo { get; set; } = true;
    public List<ManualAllocationApiRequest> ManualAllocations { get; set; } = [];
}

public class ManualAllocationApiRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}
