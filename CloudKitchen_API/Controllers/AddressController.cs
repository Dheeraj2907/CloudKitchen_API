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
public class AddressController : ControllerBase
{
    private readonly IAddressService _svc;
    public AddressController(IAddressService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(new ApiResponse { Success = true, Data = await _svc.GetAsync(User.GetUserId()) });

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddressCreateRequest req)
    {
        try { return Ok(new ApiResponse { Success = true, Data = await _svc.AddAsync(User.GetUserId(), req) }); }
        catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AddressCreateRequest req)
    {
        try { return Ok(new ApiResponse { Success = true, Data = await _svc.UpdateAsync(User.GetUserId(), id, req) }); }
        catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try { await _svc.DeleteAsync(User.GetUserId(), id); return Ok(new ApiResponse { Success = true }); }
        catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpPut("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        try { await _svc.SetDefaultAsync(User.GetUserId(), id); return Ok(new ApiResponse { Success = true }); }
        catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
    }
}