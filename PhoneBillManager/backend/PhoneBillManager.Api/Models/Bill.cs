namespace PhoneBillManager.Api.Models;

public class Bill
{
    public int BillId { get; set; }
    public int UserId { get; set; }
    public string BillFileName { get; set; } = string.Empty;
    public string BillFilePath { get; set; } = string.Empty;
    public string? BillingPeriod { get; set; }
    public DateOnly? BillingDate { get; set; }
    public string? AccountNumber { get; set; }
    public string? VendorName { get; set; }
    public decimal TotalBillAmount { get; set; }
    public decimal TotalPlanAmount { get; set; }
    public decimal TotalEquipmentAmount { get; set; }
    public decimal TotalServicesAmount { get; set; }
    public int NumberOfLines { get; set; }
    public string ParseStatus { get; set; } = "Pending";
    public DateTime? ParsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = null!;
    public ICollection<AccountLine> AccountLines { get; set; } = new List<AccountLine>();
    public ICollection<PlanCharge> PlanCharges { get; set; } = new List<PlanCharge>();
    public ICollection<EquipmentCharge> EquipmentCharges { get; set; } = new List<EquipmentCharge>();
    public ICollection<ServiceCharge> ServiceCharges { get; set; } = new List<ServiceCharge>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
