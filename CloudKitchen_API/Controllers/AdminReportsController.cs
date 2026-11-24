using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudKitchen_API.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/admin/reports")]
[ApiController]
public class AdminReportsController : ControllerBase
{
    private readonly IReportService _svc;
    public AdminReportsController(IReportService svc) => _svc = svc;

    private (DateTime from, DateTime to) ParseRange(DateTime? from, DateTime? to)
    {
        var t = to ?? DateTime.UtcNow;
        var f = from ?? t.AddDays(-30);
        if (f > t) throw new ArgumentException("From must be before To.");
        return (f.Date, t.Date.AddDays(1));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var (f, t) = ParseRange(from, to);
        return Ok(new ApiResponse { Success = true, Data = await _svc.GetSummaryAsync(f, t) });
    }

    [HttpGet("sales-by-day")]
    public async Task<IActionResult> SalesByDay([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var (f, t) = ParseRange(from, to);
        return Ok(new ApiResponse { Success = true, Data = await _svc.GetSalesByDayAsync(f, t) });
    }

    [HttpGet("sales-by-item")]
    public async Task<IActionResult> SalesByItem([FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int limit = 20)
    {
        var (f, t) = ParseRange(from, to);
        return Ok(new ApiResponse { Success = true, Data = await _svc.GetSalesByItemAsync(f, t, limit) });
    }

    [HttpGet("peak-hours")]
    public async Task<IActionResult> PeakHours([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var (f, t) = ParseRange(from, to);
        return Ok(new ApiResponse { Success = true, Data = await _svc.GetPeakHoursAsync(f, t) });
    }
}