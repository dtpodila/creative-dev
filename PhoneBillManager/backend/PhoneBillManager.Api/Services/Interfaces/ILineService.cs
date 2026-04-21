using PhoneBillManager.Api.DTOs.Bills;
using PhoneBillManager.Api.DTOs.Lines;

namespace PhoneBillManager.Api.Services.Interfaces;

public interface ILineService
{
    Task<List<LineDetailDto>> GetLinesAsync(int billId, int userId);
    Task<LineDetailDto?> AssignLineAsync(int lineId, int userId, AssignLineRequest request);
}
