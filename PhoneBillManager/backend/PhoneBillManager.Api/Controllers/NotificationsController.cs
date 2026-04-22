using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneBillManager.Api.Services.Interfaces;

namespace PhoneBillManager.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    /// <summary>
    /// Get notification history for a bill.
    /// Note: New bills sent via native device apps (SMS/WhatsApp) won't appear here.
    /// This endpoint only returns historical notifications sent via the old Twilio integration.
    /// </summary>
    [HttpGet("{billId:int}")]
    public async Task<IActionResult> GetNotifications(int billId)
    {
        var userId = GetUserId();
        var notifications = await _notifications.GetNotificationsAsync(billId, userId);
        return Ok(notifications);
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User not authenticated."));
}
