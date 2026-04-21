namespace PhoneBillManager.Api.DTOs.Bills;

public class BillDetailDto
{
    public int BillId { get; set; }
    public string? BillingPeriod { get; set; }
    public DateOnly? BillingDate { get; set; }
    public string? AccountNumber { get; set; }
    public string? VendorName { get; set; }
    public decimal TotalBillAmount { get; set; }
    public decimal TotalPlanAmount { get; set; }
    public decimal TotalEquipmentAmount { get; set; }
    public decimal TotalServicesAmount { get; set; }
    public int NumberOfLines { get; set; }
    public string ParseStatus { get; set; } = string.Empty;

    public List<LineDetailDto> Lines { get; set; } = new();
    public List<PlanChargeDto> PlanCharges { get; set; } = new();
}

public class LineDetailDto
{
    public int LineId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? LineLabel { get; set; }
    public string? AssignedName { get; set; }
    public string? AssignedContact { get; set; }
    public decimal PlanCostShare { get; set; }
    public decimal EquipmentCost { get; set; }
    public decimal ServicesCost { get; set; }
    public decimal TotalLineCost { get; set; }
    public List<EquipmentChargeDto> EquipmentCharges { get; set; } = new();
    public List<ServiceChargeDto> ServiceCharges { get; set; } = new();
}

public class PlanChargeDto
{
    public int PlanChargeId { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public decimal ChargeAmount { get; set; }
    public string? ChargeType { get; set; }
}

public class EquipmentChargeDto
{
    public int EquipmentChargeId { get; set; }
    public string? DeviceName { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public decimal ChargeAmount { get; set; }
    public string? ChargeType { get; set; }
}

public class ServiceChargeDto
{
    public int ServiceChargeId { get; set; }
    public string ChargeName { get; set; } = string.Empty;
    public decimal ChargeAmount { get; set; }
    public string? ChargeType { get; set; }
}
