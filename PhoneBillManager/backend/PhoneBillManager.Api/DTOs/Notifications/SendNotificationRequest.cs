using System.ComponentModel.DataAnnotations;

namespace PhoneBillManager.Api.DTOs.Notifications;

public class SendNotificationRequest
{
    [Required]
    public string Channel { get; set; } = "SMS"; // SMS | WhatsApp
}

public class NotificationDto
{
    public int NotificationId { get; set; }
    public int LineId { get; set; }
    public string? RecipientName { get; set; }
    public string RecipientContact { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}
