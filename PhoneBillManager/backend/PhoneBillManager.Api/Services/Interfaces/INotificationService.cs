using PhoneBillManager.Api.DTOs.Notifications;

namespace PhoneBillManager.Api.Services.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> SendBillAsync(int billId, int userId, string channel);
    Task<NotificationDto?> SendLineAsync(int lineId, int userId, string channel);
    Task<List<NotificationDto>> GetNotificationsAsync(int billId, int userId);
}
