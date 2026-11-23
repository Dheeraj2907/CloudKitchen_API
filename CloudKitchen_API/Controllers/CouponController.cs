using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudKitchen_API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CouponController : ControllerBase
{
    private readonly ICouponService _svc;
    public CouponController(ICouponService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(new ApiResponse { Success = true, Data = await _svc.GetActiveAsync() });

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateCouponRequest req)
        => Ok(new ApiResponse { Success = true, Data = await _svc.ValidateAsync(req) });
}