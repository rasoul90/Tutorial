using DeliverySaaS.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/branch/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public Task<DeliverySaaS.Application.Common.Models.PagedResult<NotificationDto>> Get([FromQuery] bool unreadOnly = true, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken cancellationToken = default)
        => _notificationService.GetBranchNotificationsAsync(unreadOnly, page, size, cancellationToken);

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkReadAsync(id, cancellationToken);
        return Ok(new { message = "Read" });
    }
}
