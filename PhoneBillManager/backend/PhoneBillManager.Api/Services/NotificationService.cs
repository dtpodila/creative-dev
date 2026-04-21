using Microsoft.EntityFrameworkCore;
using PhoneBillManager.Api.Data;
using PhoneBillManager.Api.DTOs.Notifications;
using PhoneBillManager.Api.Models;
using PhoneBillManager.Api.Services.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace PhoneBillManager.Api.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public NotificationService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
        TwilioClient.Init(
            _config["Twilio:AccountSid"],
            _config["Twilio:AuthToken"]
        );
    }

    public async Task<List<NotificationDto>> SendBillAsync(int billId, int userId, string channel)
    {
        var bill = await _db.Bills
            .Include(b => b.AccountLines)
            .FirstOrDefaultAsync(b => b.BillId == billId && b.UserId == userId);

        if (bill == null) return new List<NotificationDto>();

        var results = new List<NotificationDto>();
        foreach (var line in bill.AccountLines.Where(l => !string.IsNullOrWhiteSpace(l.AssignedContact)))
        {
            var dto = await SendToLineAsync(bill, line, channel);
            results.Add(dto);
        }
        return results;
    }

    public async Task<NotificationDto?> SendLineAsync(int lineId, int userId, string channel)
    {
        var line = await _db.AccountLines
            .Include(l => l.Bill)
            .FirstOrDefaultAsync(l => l.LineId == lineId && l.Bill.UserId == userId);

        if (line == null || string.IsNullOrWhiteSpace(line.AssignedContact)) return null;

        return await SendToLineAsync(line.Bill, line, channel);
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

    private async Task<NotificationDto> SendToLineAsync(Bill bill, AccountLine line, string channel)
    {
        var name = line.AssignedName ?? "there";
        var body = BuildMessage(name, bill, line);

        var notification = new Notification
        {
            BillId = bill.BillId,
            LineId = line.LineId,
            RecipientName = line.AssignedName,
            RecipientContact = line.AssignedContact!,
            Channel = channel,
            MessageBody = body,
            Status = "Pending"
        };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        try
        {
            var from = channel.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase)
                ? _config["Twilio:WhatsAppFrom"]
                : _config["Twilio:FromPhoneNumber"];

            var to = channel.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase)
                ? $"whatsapp:{line.AssignedContact}"
                : line.AssignedContact!;

            var message = await MessageResource.CreateAsync(
                body: body,
                from: new Twilio.Types.PhoneNumber(from),
                to: new Twilio.Types.PhoneNumber(to)
            );

            notification.Status = "Sent";
            notification.ProviderMessageId = message.Sid;
            notification.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            notification.Status = "Failed";
            notification.ErrorMessage = ex.Message;
        }

        await _db.SaveChangesAsync();

        return new NotificationDto
        {
            NotificationId = notification.NotificationId,
            LineId = notification.LineId,
            RecipientName = notification.RecipientName,
            RecipientContact = notification.RecipientContact,
            Channel = notification.Channel,
            Status = notification.Status,
            SentAt = notification.SentAt,
            ErrorMessage = notification.ErrorMessage
        };
    }

    private static string BuildMessage(string name, Bill bill, AccountLine line)
    {
        var period = string.IsNullOrWhiteSpace(bill.BillingPeriod) ? "this period" : bill.BillingPeriod;
        return $"""
Hi {name}! 👋

Here is your phone bill summary for: {period}

📱 Line: {line.PhoneNumber}

💰 Cost Breakdown:
   Plan Share:   ${line.PlanCostShare:F2}
   Equipment:    ${line.EquipmentCost:F2}
   Services:     ${line.ServicesCost:F2}
   ─────────────────────
   YOUR TOTAL:   ${line.TotalLineCost:F2}

Please arrange payment at your earliest convenience.
Thank you!
""";
    }
}
