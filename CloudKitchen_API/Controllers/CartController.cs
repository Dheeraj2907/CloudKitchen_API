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
public class CartController : ControllerBase
{
    private readonly ICartService _cart;
    public CartController(ICartService cart) => _cart = cart;

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(new ApiResponse { Success = true, Data = await _cart.GetCartAsync(User.GetUserId()) });

    [HttpPost("items")]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request)
    {
        try { return Ok(new ApiResponse { Success = true, Data = await _cart.AddToCartAsync(User.GetUserId(), request) }); }
        catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCartRequest request)
    {
        try { return Ok(new ApiResponse { Success = true, Data = await _cart.UpdateQuantityAsync(User.GetUserId(), id, request.Quantity) }); }
        catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        try { await _cart.RemoveItemAsync(User.GetUserId(), id); return Ok(new ApiResponse { Success = true }); }
        catch (ApiException ex) { return NotFound(new ApiResponse { Success = false, Message = ex.Message }); }
    }
}