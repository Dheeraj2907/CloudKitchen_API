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
public class RiderController : ControllerBase
{
    private readonly IRiderService _riderService;
    public RiderController(IRiderService riderService) => _riderService = riderService;

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetForOrder(Guid orderId)
    {
        try
        {
            var rider = await _riderService.GetRiderForOrderAsync(User.GetUserId(), orderId);
            return rider == null
                ? Ok(new ApiResponse { Success = true, Data = null, Message = "No rider assigned yet." })
                : Ok(new ApiResponse { Success = true, Data = rider });
        }
        catch (ApiException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
    }
}