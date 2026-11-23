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
public class UserController : ControllerBase
{
    private readonly IUserService _svc;
    public UserController(IUserService svc) => _svc = svc;

    [HttpGet("profile")]
    public async Task<IActionResult> Get()
    {
        try { return Ok(new ApiResponse { Success = true, Data = await _svc.GetProfileAsync(User.GetUserId()) }); }
        catch (ApiException ex) { return NotFound(new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest req)
    {
        try { return Ok(new ApiResponse { Success = true, Data = await _svc.UpdateProfileAsync(User.GetUserId(), req) }); }
        catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
    }
}