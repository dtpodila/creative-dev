namespace PhoneBillManager.Api.DTOs.Bills;

public class BillSummaryDto
{
    public int BillId { get; set; }
    public string BillFileName { get; set; } = string.Empty;
    public string? BillingPeriod { get; set; }
    public DateOnly? BillingDate { get; set; }
    public string? VendorName { get; set; }
    public string? AccountNumber { get; set; }
    public decimal TotalBillAmount { get; set; }
    public int NumberOfLines { get; set; }
    public string ParseStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
