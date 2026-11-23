using CloudKitchen_API.Extensions;
using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudKitchen_API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _svc;
    public ReviewController(IReviewService svc) => _svc = svc;

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitReviewRequest req)
    {
        try { return Ok(new ApiResponse { Success = true, Data = await _svc.SubmitAsync(User.GetUserId(), req) }); }
        catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpGet("item/{menuItemId:guid}")]
    public async Task<IActionResult> ForItem(Guid menuItemId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(new ApiResponse { Success = true, Data = await _svc.GetForItemAsync(menuItemId, page, pageSize) });

    [HttpGet("my")]
    public async Task<IActionResult> Mine()
        => Ok(new ApiResponse { Success = true, Data = await _svc.GetMineAsync(User.GetUserId()) });
}