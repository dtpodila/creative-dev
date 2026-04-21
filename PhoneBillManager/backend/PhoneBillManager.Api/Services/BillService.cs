using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PhoneBillManager.Api.Data;
using PhoneBillManager.Api.DTOs.Bills;
using PhoneBillManager.Api.Models;
using PhoneBillManager.Api.Services.Interfaces;

namespace PhoneBillManager.Api.Services;

public class BillService : IBillService
{
    private readonly AppDbContext _db;
    private readonly IBillParserService _parser;
    private readonly IConfiguration _config;

    public BillService(AppDbContext db, IBillParserService parser, IConfiguration config)
    {
        _db = db;
        _parser = parser;
        _config = config;
    }

    public async Task<List<BillSummaryDto>> GetBillsAsync(int userId)
    {
        return await _db.Bills
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BillSummaryDto
            {
                BillId = b.BillId,
                BillFileName = b.BillFileName,
                BillingPeriod = b.BillingPeriod,
                BillingDate = b.BillingDate,
                VendorName = b.VendorName,
                AccountNumber = b.AccountNumber,
                TotalBillAmount = b.TotalBillAmount,
                NumberOfLines = b.NumberOfLines,
                ParseStatus = b.ParseStatus,
                CreatedAt = b.CreatedAt
            }).ToListAsync();
    }

    public async Task<BillDetailDto?> GetBillDetailAsync(int billId, int userId)
    {
        var bill = await _db.Bills
            .Include(b => b.PlanCharges)
            .Include(b => b.AccountLines)
                .ThenInclude(l => l.EquipmentCharges)
            .Include(b => b.AccountLines)
                .ThenInclude(l => l.ServiceCharges)
            .FirstOrDefaultAsync(b => b.BillId == billId && b.UserId == userId);

        if (bill == null) return null;

        return new BillDetailDto
        {
            BillId = bill.BillId,
            BillingPeriod = bill.BillingPeriod,
            BillingDate = bill.BillingDate,
            AccountNumber = bill.AccountNumber,
            VendorName = bill.VendorName,
            TotalBillAmount = bill.TotalBillAmount,
            TotalPlanAmount = bill.TotalPlanAmount,
            TotalEquipmentAmount = bill.TotalEquipmentAmount,
            TotalServicesAmount = bill.TotalServicesAmount,
            NumberOfLines = bill.NumberOfLines,
            ParseStatus = bill.ParseStatus,
            PlanCharges = bill.PlanCharges.OrderBy(p => p.SortOrder).Select(p => new PlanChargeDto
            {
                PlanChargeId = p.PlanChargeId,
                ChargeName = p.ChargeName,
                ChargeAmount = p.ChargeAmount,
                ChargeType = p.ChargeType
            }).ToList(),
            Lines = bill.AccountLines.OrderBy(l => l.SortOrder).Select(l => new LineDetailDto
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
            }).ToList()
        };
    }

    public async Task<BillSummaryDto> UploadAndParseAsync(IFormFile file, int userId)
    {
        // Save file to disk
        var uploadPath = _config["FileStorage:UploadPath"] ?? "uploads/bills";
        Directory.CreateDirectory(uploadPath);
        var safeFileName = $"{Guid.NewGuid()}.pdf";
        var fullPath = Path.Combine(uploadPath, safeFileName);

        await using (var fs = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(fs);

        // Create bill record
        var bill = new Bill
        {
            UserId = userId,
            BillFileName = Path.GetFileName(file.FileName),
            BillFilePath = fullPath,
            ParseStatus = "Processing"
        };
        _db.Bills.Add(bill);
        await _db.SaveChangesAsync();

        try
        {
            // Parse the PDF
            await using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            var parsed = await _parser.ParseAsync(stream);

            // Populate bill header
            bill.BillingPeriod = parsed.BillingPeriod;
            bill.AccountNumber = parsed.AccountNumber;
            bill.NumberOfLines = parsed.LineCharges.Count;
            bill.TotalPlanAmount = parsed.TotalPlanAmount;
            bill.TotalEquipmentAmount = parsed.LineCharges.Sum(l => l.EquipmentAmount);
            bill.TotalServicesAmount = parsed.LineCharges.Sum(l => l.ServicesAmount);
            bill.TotalBillAmount = parsed.LineCharges.Sum(l => l.TotalAmount);

            // Save plan charges
            for (var i = 0; i < parsed.PlanCharges.Count; i++)
            {
                var (name, amount, type) = parsed.PlanCharges[i];
                _db.PlanCharges.Add(new PlanCharge
                {
                    BillId = bill.BillId,
                    ChargeName = name,
                    ChargeAmount = amount,
                    ChargeType = type,
                    SortOrder = i
                });
            }

            // Save account lines with per-line charges from parsed data
            var lineMap = new Dictionary<string, AccountLine>();
            for (var i = 0; i < parsed.LineCharges.Count; i++)
            {
                var (phone, plan, equip, svc, total) = parsed.LineCharges[i];
                var line = new AccountLine
                {
                    BillId = bill.BillId,
                    PhoneNumber = phone,
                    PlanCostShare = plan,
                    EquipmentCost = equip,
                    ServicesCost = svc,
                    TotalLineCost = total,
                    SortOrder = i
                };
                _db.AccountLines.Add(line);
                lineMap[phone] = line;
            }
            await _db.SaveChangesAsync();

            // Save equipment charges
            for (var i = 0; i < parsed.EquipmentCharges.Count; i++)
            {
                var (phone, device, name, amount, type) = parsed.EquipmentCharges[i];
                lineMap.TryGetValue(phone, out var line);
                _db.EquipmentCharges.Add(new EquipmentCharge
                {
                    BillId = bill.BillId,
                    LineId = line?.LineId,
                    PhoneNumber = phone,
                    DeviceName = device,
                    ChargeName = name,
                    ChargeAmount = amount,
                    ChargeType = type,
                    SortOrder = i
                });
            }

            // Save service charges
            for (var i = 0; i < parsed.ServiceCharges.Count; i++)
            {
                var (phone, name, amount, type) = parsed.ServiceCharges[i];
                lineMap.TryGetValue(phone, out var line);
                _db.ServiceCharges.Add(new ServiceCharge
                {
                    BillId = bill.BillId,
                    LineId = line?.LineId,
                    PhoneNumber = phone,
                    ChargeName = name,
                    ChargeAmount = amount,
                    ChargeType = type,
                    SortOrder = i
                });
            }

            bill.ParseStatus = "Parsed";
            bill.ParsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            bill.ParseStatus = "Failed";
            await _db.SaveChangesAsync();
            throw new InvalidOperationException($"PDF parsing failed: {ex.Message}", ex);
        }

        return new BillSummaryDto
        {
            BillId = bill.BillId,
            BillFileName = bill.BillFileName,
            BillingPeriod = bill.BillingPeriod,
            BillingDate = bill.BillingDate,
            VendorName = bill.VendorName,
            AccountNumber = bill.AccountNumber,
            TotalBillAmount = bill.TotalBillAmount,
            NumberOfLines = bill.NumberOfLines,
            ParseStatus = bill.ParseStatus,
            CreatedAt = bill.CreatedAt
        };
    }

    public async Task<bool> DeleteBillAsync(int billId, int userId)
    {
        var bill = await _db.Bills.FirstOrDefaultAsync(b => b.BillId == billId && b.UserId == userId);
        if (bill == null) return false;

        if (File.Exists(bill.BillFilePath))
            File.Delete(bill.BillFilePath);

        _db.Bills.Remove(bill);
        await _db.SaveChangesAsync();
        return true;
    }
}
