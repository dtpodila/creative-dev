namespace PhoneBillManager.Api.Models;

public class PlanCharge
{
    public int PlanChargeId { get; set; }
    public int BillId { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public decimal ChargeAmount { get; set; }
    public string? ChargeType { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Bill Bill { get; set; } = null!;
}
