using DeliverySaaS.Application.Printing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/branch/print")]
[Authorize]
public class BranchPrintController : ControllerBase
{
    private readonly ILabelService _labelService;

    public BranchPrintController(ILabelService labelService)
    {
        _labelService = labelService;
    }

    [HttpPost("labels")]
    public IActionResult PrintLabels([FromBody] GenerateLabelsRequest request)
    {
        var pdf = _labelService.GenerateLabelsPdf(request);
        return File(pdf, "application/pdf", "labels.pdf");
    }
}
