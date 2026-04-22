using PhoneBillManager.Api.DTOs.Notifications;

namespace PhoneBillManager.Api.Services.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(int billId, int userId);
}
