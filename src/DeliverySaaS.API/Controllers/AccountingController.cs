using DeliverySaaS.Application.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/accounting")]
[Authorize]
public class AccountingController : ControllerBase
{
    private readonly IAccountingService _accountingService;

    public AccountingController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    [HttpPost("settlements")]
    public async Task<IActionResult> CreateSettlement([FromBody] CreateSettlementRequest request, CancellationToken cancellationToken)
    {
        var id = await _accountingService.CreateMerchantSettlementRequestAsync(request.MerchantId, request.Amount, cancellationToken);
        return Ok(new { settlementRequestId = id });
    }

    [HttpPost("settlements/{id:guid}/approve-generate-invoice")]
    public async Task<IActionResult> ApproveSettlementAndGenerateInvoice([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var invoiceId = await _accountingService.ApproveSettlementAndGenerateInvoiceAsync(id, cancellationToken);
        return Ok(new { invoiceId });
    }

    [HttpPost("invoices/generate")]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var id = await _accountingService.GenerateMerchantInvoiceAsync(request.MerchantId, request.TotalAmount, cancellationToken);
        return Ok(new { invoiceId = id });
    }

    [HttpPost("reconciliations")]
    public async Task<IActionResult> RecordReconciliation([FromBody] CreateReconciliationRequest request, CancellationToken cancellationToken)
    {
        var id = await _accountingService.RecordDeliveryReconciliationAsync(request.DeliveryAgentId, request.CollectedAmount, request.DeliveredAmount, cancellationToken);
        return Ok(new { reconciliationId = id });
    }

    [HttpPost("payroll")]
    public async Task<IActionResult> CreatePayroll([FromBody] CreatePayrollRequest request, CancellationToken cancellationToken)
    {
        var id = await _accountingService.CreatePayrollAsync(request.UserId, request.Amount, request.PayrollDate, cancellationToken);
        return Ok(new { payrollId = id });
    }


    [HttpGet("delivery-agent-settlement/pending-orders")]
    public async Task<IActionResult> PendingDeliveryAgentSettlementOrders(CancellationToken cancellationToken)
    {
        var data = await _accountingService.GetOrdersPendingDeliveryAgentSettlementAsync(cancellationToken);
        return Ok(data.Select(x => new
        {
            x.Id,
            x.OrderNumber,
            x.DeliveredAt,
            x.DeliveryFeeApplied,
            x.DeliveryAgentFeeApplied,
            x.MerchantDueAmount,
            x.CompanyNetDeliveryProfit
        }));
    }

    [HttpGet("merchant-settlement/available-orders")]
    public async Task<IActionResult> AvailableMerchantSettlementOrders(CancellationToken cancellationToken)
    {
        var data = await _accountingService.GetOrdersAvailableForMerchantSettlementAsync(cancellationToken);
        return Ok(data.Select(x => new
        {
            x.Id,
            x.OrderNumber,
            x.DeliveredAt,
            x.DeliveryFeeApplied,
            x.DeliveryAgentFeeApplied,
            x.MerchantDueAmount,
            x.CompanyNetDeliveryProfit
        }));
    }

    [HttpGet("reports/company-net-delivery-profit")]
    public async Task<IActionResult> CompanyNetDeliveryProfit([FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var sum = await _accountingService.GetCompanyNetDeliveryProfitAsync(branchId, cancellationToken);
        return Ok(new { companyNetDeliveryProfit = sum });
    }

    [HttpPost("expenses")]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        var id = await _accountingService.CreateExpenseAsync(request.Category, request.Amount, request.Notes, cancellationToken);
        return Ok(new { expenseId = id });
    }
}

public class CreateSettlementRequest
{
    public Guid MerchantId { get; set; }
    public decimal Amount { get; set; }
}

public class GenerateInvoiceRequest
{
    public Guid MerchantId { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CreateReconciliationRequest
{
    public Guid DeliveryAgentId { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal DeliveredAmount { get; set; }
}

public class CreatePayrollRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PayrollDate { get; set; }
}

public class CreateExpenseRequest
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
