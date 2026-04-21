using PhoneBillManager.Api.DTOs.Bills;
using Microsoft.AspNetCore.Http;

namespace PhoneBillManager.Api.Services.Interfaces;

public interface IBillService
{
    Task<List<BillSummaryDto>> GetBillsAsync(int userId);
    Task<BillDetailDto?> GetBillDetailAsync(int billId, int userId);
    Task<BillSummaryDto> UploadAndParseAsync(IFormFile file, int userId);
    Task<bool> DeleteBillAsync(int billId, int userId);
}
