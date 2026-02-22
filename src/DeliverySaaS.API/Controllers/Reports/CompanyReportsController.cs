using DeliverySaaS.API.Authorization;
using DeliverySaaS.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers.Reports;

[ApiController]
[Route("api/company")]
[Authorize]
public class CompanyReportsController : ControllerBase
{
    private readonly IFinancialReportsQueryService _financialReportsQueryService;

    public CompanyReportsController(IFinancialReportsQueryService financialReportsQueryService)
    {
        _financialReportsQueryService = financialReportsQueryService;
    }

    [HttpGet("reports/financial-summary")]
    [Authorize(Policy = AuthorizationPolicies.CanViewCompanyReports)]
    public Task<BranchFinancialSummaryDto> FinancialSummary([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
        => _financialReportsQueryService.GetCompanyFinancialSummaryAsync(from.Date, to.Date.AddDays(1).AddTicks(-1), branchId, cancellationToken);
}
