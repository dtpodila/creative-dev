namespace PhoneBillManager.Api.Models;

public class EquipmentCharge
{
    public int EquipmentChargeId { get; set; }
    public int BillId { get; set; }
    public int? LineId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? DeviceName { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public decimal ChargeAmount { get; set; }
    public string? ChargeType { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Bill Bill { get; set; } = null!;
    public AccountLine? AccountLine { get; set; }
}
