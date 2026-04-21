namespace PhoneBillManager.Api.Models;

public class AccountLine
{
    public int LineId { get; set; }
    public int BillId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? LineLabel { get; set; }
    public string? AssignedName { get; set; }
    public string? AssignedContact { get; set; }
    public decimal PlanCostShare { get; set; }
    public decimal EquipmentCost { get; set; }
    public decimal ServicesCost { get; set; }
    public decimal TotalLineCost { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Bill Bill { get; set; } = null!;
    public ICollection<EquipmentCharge> EquipmentCharges { get; set; } = new List<EquipmentCharge>();
    public ICollection<ServiceCharge> ServiceCharges { get; set; } = new List<ServiceCharge>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
