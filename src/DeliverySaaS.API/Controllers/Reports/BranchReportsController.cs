using DeliverySaaS.API.Authorization;
using DeliverySaaS.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers.Reports;

[ApiController]
[Route("api/branch")]
[Authorize]
public class BranchReportsController : ControllerBase
{
    private readonly IFinancialReportsQueryService _financialReportsQueryService;
    private readonly IMerchantStatementsQueryService _merchantStatementsQueryService;
    private readonly IDeliveryAgentStatementsQueryService _deliveryAgentStatementsQueryService;

    public BranchReportsController(
        IFinancialReportsQueryService financialReportsQueryService,
        IMerchantStatementsQueryService merchantStatementsQueryService,
        IDeliveryAgentStatementsQueryService deliveryAgentStatementsQueryService)
    {
        _financialReportsQueryService = financialReportsQueryService;
        _merchantStatementsQueryService = merchantStatementsQueryService;
        _deliveryAgentStatementsQueryService = deliveryAgentStatementsQueryService;
    }

    [HttpGet("reports/financial-summary")]
    [Authorize(Policy = AuthorizationPolicies.CanViewFinancialReports)]
    public Task<BranchFinancialSummaryDto> FinancialSummary([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
        => _financialReportsQueryService.GetBranchFinancialSummaryAsync(from.Date, to.Date.AddDays(1).AddTicks(-1), cancellationToken);

    [HttpGet("merchants/{merchantId:guid}/statement")]
    [Authorize(Policy = AuthorizationPolicies.CanViewFinancialReports)]
    public Task<StatementResultDto<MerchantStatementSummaryDto, MerchantStatementRowDto>> MerchantStatement(
        [FromRoute] Guid merchantId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] bool onlyReadyForSettlement = false,
        CancellationToken cancellationToken = default)
        => _merchantStatementsQueryService.GetStatementAsync(merchantId, from.Date, to.Date.AddDays(1).AddTicks(-1), page, size, onlyReadyForSettlement, cancellationToken);

    [HttpGet("delivery-agents/{agentId:guid}/statement")]
    [Authorize(Policy = AuthorizationPolicies.CanViewFinancialReports)]
    public Task<StatementResultDto<DeliveryAgentStatementSummaryDto, DeliveryAgentStatementRowDto>> DeliveryAgentStatement(
        [FromRoute] Guid agentId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] bool onlyUnsettled = false,
        CancellationToken cancellationToken = default)
        => _deliveryAgentStatementsQueryService.GetStatementAsync(agentId, from.Date, to.Date.AddDays(1).AddTicks(-1), page, size, onlyUnsettled, cancellationToken);
}
