using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneBillManager.Api.DTOs.Notifications;
using PhoneBillManager.Api.Services.Interfaces;

namespace PhoneBillManager.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpPost("send/{billId:int}")]
    public async Task<IActionResult> SendBill(int billId, [FromBody] SendNotificationRequest request)
    {
        var userId = GetUserId();
        var results = await _notifications.SendBillAsync(billId, userId, request.Channel);
        return Ok(results);
    }

    [HttpPost("send-line/{lineId:int}")]
    public async Task<IActionResult> SendLine(int lineId, [FromBody] SendNotificationRequest request)
    {
        var userId = GetUserId();
        var result = await _notifications.SendLineAsync(lineId, userId, request.Channel);
        if (result == null) return NotFound(new { message = "Line not found or no contact assigned." });
        return Ok(result);
    }

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
