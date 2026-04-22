using Microsoft.EntityFrameworkCore;
using PhoneBillManager.Api.Data;
using PhoneBillManager.Api.DTOs.Notifications;
using PhoneBillManager.Api.Services.Interfaces;

namespace PhoneBillManager.Api.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(int billId, int userId)
    {
        var billExists = await _db.Bills.AnyAsync(b => b.BillId == billId && b.UserId == userId);
        if (!billExists) return new List<NotificationDto>();

        return await _db.Notifications
            .Where(n => n.BillId == billId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                LineId = n.LineId,
                RecipientName = n.RecipientName,
                RecipientContact = n.RecipientContact,
                Channel = n.Channel,
                Status = n.Status,
                SentAt = n.SentAt,
                ErrorMessage = n.ErrorMessage
            }).ToListAsync();
    }
}
