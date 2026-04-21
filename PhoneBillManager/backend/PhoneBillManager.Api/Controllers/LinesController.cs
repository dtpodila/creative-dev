using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneBillManager.Api.DTOs.Lines;
using PhoneBillManager.Api.Services.Interfaces;

namespace PhoneBillManager.Api.Controllers;

[ApiController]
[Authorize]
public class LinesController : ControllerBase
{
    private readonly ILineService _lines;

    public LinesController(ILineService lines) => _lines = lines;

    [HttpGet("api/bills/{billId:int}/lines")]
    public async Task<IActionResult> GetLines(int billId)
    {
        var userId = GetUserId();
        var lines = await _lines.GetLinesAsync(billId, userId);
        return Ok(lines);
    }

    [HttpPut("api/lines/{lineId:int}/assign")]
    public async Task<IActionResult> AssignLine(int lineId, [FromBody] AssignLineRequest request)
    {
        var userId = GetUserId();
        var line = await _lines.AssignLineAsync(lineId, userId, request);
        if (line == null) return NotFound();
        return Ok(line);
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User not authenticated."));
}
