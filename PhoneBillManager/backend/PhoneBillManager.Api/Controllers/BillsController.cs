using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneBillManager.Api.Services.Interfaces;

namespace PhoneBillManager.Api.Controllers;

[ApiController]
[Route("api/bills")]
[Authorize]
public class BillsController : ControllerBase
{
    private readonly IBillService _bills;
    private readonly IBillParserService _parser;

    public BillsController(IBillService bills, IBillParserService parser) 
    { 
        _bills = bills;
        _parser = parser;
    }

    [HttpGet]
    public async Task<IActionResult> GetBills()
    {
        var userId = GetUserId();
        var bills = await _bills.GetBillsAsync(userId);
        return Ok(bills);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBill(int id)
    {
        var userId = GetUserId();
        var bill = await _bills.GetBillDetailAsync(id, userId);
        if (bill == null) return NotFound();
        return Ok(bill);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only PDF files are accepted." });

        if (file.Length > 20 * 1024 * 1024)
            return BadRequest(new { message = "File size exceeds 20 MB limit." });

        try
        {
            var userId = GetUserId();
            var result = await _bills.UploadAndParseAsync(file, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBill(int id)
    {
        var userId = GetUserId();
        var deleted = await _bills.DeleteBillAsync(id, userId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("debug/extract-lines")]
    public async Task<IActionResult> DebugExtractLines(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        try
        {
            using var stream = file.OpenReadStream();
            var lines = await _parser.GetDebugLinesAsync(stream);
            return Ok(new { totalLines = lines.Count, lines });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User not authenticated."));
}
