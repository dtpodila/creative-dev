using Microsoft.EntityFrameworkCore;
using PhoneBillManager.Api.Data;
using PhoneBillManager.Api.DTOs.Bills;
using PhoneBillManager.Api.DTOs.Lines;
using PhoneBillManager.Api.Services.Interfaces;

namespace PhoneBillManager.Api.Services;

public class LineService : ILineService
{
    private readonly AppDbContext _db;

    public LineService(AppDbContext db) => _db = db;

    public async Task<List<LineDetailDto>> GetLinesAsync(int billId, int userId)
    {
        var billExists = await _db.Bills.AnyAsync(b => b.BillId == billId && b.UserId == userId);
        if (!billExists) return new List<LineDetailDto>();

        return await _db.AccountLines
            .Where(l => l.BillId == billId)
            .Include(l => l.EquipmentCharges)
            .Include(l => l.ServiceCharges)
            .OrderBy(l => l.SortOrder)
            .Select(l => new LineDetailDto
            {
                LineId = l.LineId,
                PhoneNumber = l.PhoneNumber,
                LineLabel = l.LineLabel,
                AssignedName = l.AssignedName,
                AssignedContact = l.AssignedContact,
                PlanCostShare = l.PlanCostShare,
                EquipmentCost = l.EquipmentCost,
                ServicesCost = l.ServicesCost,
                TotalLineCost = l.TotalLineCost,
                EquipmentCharges = l.EquipmentCharges.Select(e => new EquipmentChargeDto
                {
                    EquipmentChargeId = e.EquipmentChargeId,
                    DeviceName = e.DeviceName,
                    ChargeName = e.ChargeName,
                    ChargeAmount = e.ChargeAmount,
                    ChargeType = e.ChargeType
                }).ToList(),
                ServiceCharges = l.ServiceCharges.Select(s => new ServiceChargeDto
                {
                    ServiceChargeId = s.ServiceChargeId,
                    ChargeName = s.ChargeName,
                    ChargeAmount = s.ChargeAmount,
                    ChargeType = s.ChargeType
                }).ToList()
            }).ToListAsync();
    }

    public async Task<LineDetailDto?> AssignLineAsync(int lineId, int userId, AssignLineRequest request)
    {
        var line = await _db.AccountLines
            .Include(l => l.Bill)
            .Include(l => l.EquipmentCharges)
            .Include(l => l.ServiceCharges)
            .FirstOrDefaultAsync(l => l.LineId == lineId && l.Bill.UserId == userId);

        if (line == null) return null;

        line.AssignedName = request.AssignedName.Trim();
        line.AssignedContact = request.AssignedContact.Trim();
        line.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new LineDetailDto
        {
            LineId = line.LineId,
            PhoneNumber = line.PhoneNumber,
            LineLabel = line.LineLabel,
            AssignedName = line.AssignedName,
            AssignedContact = line.AssignedContact,
            PlanCostShare = line.PlanCostShare,
            EquipmentCost = line.EquipmentCost,
            ServicesCost = line.ServicesCost,
            TotalLineCost = line.TotalLineCost,
            EquipmentCharges = line.EquipmentCharges.Select(e => new EquipmentChargeDto
            {
                EquipmentChargeId = e.EquipmentChargeId,
                DeviceName = e.DeviceName,
                ChargeName = e.ChargeName,
                ChargeAmount = e.ChargeAmount,
                ChargeType = e.ChargeType
            }).ToList(),
            ServiceCharges = line.ServiceCharges.Select(s => new ServiceChargeDto
            {
                ServiceChargeId = s.ServiceChargeId,
                ChargeName = s.ChargeName,
                ChargeAmount = s.ChargeAmount,
                ChargeType = s.ChargeType
            }).ToList()
        };
    }
}
