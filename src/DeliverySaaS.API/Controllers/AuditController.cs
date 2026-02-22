using DeliverySaaS.API.Authorization;
using DeliverySaaS.Application.Auditing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/branch/audit")]
[Authorize(Policy = AuthorizationPolicies.CanViewFinancialReports)]
public class AuditController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public Task<DeliverySaaS.Application.Common.Models.PagedResult<AuditLogDto>> Query([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? action, [FromQuery] string? entityType, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken cancellationToken = default)
        => _auditLogService.QueryAsync(from.Date, to.Date.AddDays(1).AddTicks(-1), action, entityType, page, size, cancellationToken);
}
