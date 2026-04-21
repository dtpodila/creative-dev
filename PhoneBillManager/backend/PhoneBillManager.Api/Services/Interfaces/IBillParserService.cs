using PhoneBillManager.Api.Models;

namespace PhoneBillManager.Api.Services.Interfaces;

public class ParsedBillData
{
    public string? BillingPeriod { get; set; }
    public string? BillingDate { get; set; }
    public string? AccountNumber { get; set; }
    public string? VendorName { get; set; }
    public decimal TotalPlanAmount { get; set; }
    public List<string> PhoneNumbers { get; set; } = new();
    public List<(string Phone, decimal PlanAmount, decimal EquipmentAmount, decimal ServicesAmount, decimal TotalAmount)> LineCharges { get; set; } = new();
    public List<(string Name, decimal Amount, string? Type)> PlanCharges { get; set; } = new();
    public List<(string Phone, string? Device, string Name, decimal Amount, string? Type)> EquipmentCharges { get; set; } = new();
    public List<(string Phone, string Name, decimal Amount, string? Type)> ServiceCharges { get; set; } = new();
}

public interface IBillParserService
{
    Task<ParsedBillData> ParseAsync(Stream pdfStream);
    Task<List<string>> GetDebugLinesAsync(Stream pdfStream);
}
