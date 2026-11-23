using CloudKitchen_API.Extensions;
using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudKitchen_API.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationQueryService _svc;
    public NotificationController(INotificationQueryService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(new ApiResponse { Success = true, Data = await _svc.GetAsync(User.GetUserId(), page, pageSize) });

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        try { await _svc.MarkReadAsync(User.GetUserId(), id); return Ok(new ApiResponse { Success = true }); }
        catch (ApiException ex) { return NotFound(new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _svc.MarkAllReadAsync(User.GetUserId());
        return Ok(new ApiResponse { Success = true });
    }

    [HttpPost("device")]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest req)
    {
        try { await _svc.RegisterDeviceAsync(User.GetUserId(), req.Token, req.Platform); return Ok(new ApiResponse { Success = true }); }
        catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
    }
}