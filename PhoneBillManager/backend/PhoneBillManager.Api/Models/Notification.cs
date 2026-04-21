namespace PhoneBillManager.Api.Models;

public class Notification
{
    public int NotificationId { get; set; }
    public int BillId { get; set; }
    public int LineId { get; set; }
    public string? RecipientName { get; set; }
    public string RecipientContact { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;  // WhatsApp | SMS
    public string MessageBody { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";  // Pending | Sent | Delivered | Failed
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Bill Bill { get; set; } = null!;
    public AccountLine AccountLine { get; set; } = null!;
}
