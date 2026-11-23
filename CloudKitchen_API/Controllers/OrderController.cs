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
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService) => _orderService = orderService;

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        try
        {
            var result = await _orderService.PlaceOrderAsync(User.GetUserId(), request);
            return Ok(new ApiResponse { Success = true, Data = result });
        }
        catch (ApiException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/track")]
    public async Task<IActionResult> Track(Guid id)
    {
        try
        {
            var result = await _orderService.TrackOrderAsync(User.GetUserId(), id);
            return Ok(new ApiResponse { Success = true, Data = result });
        }
        catch (ApiException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reorder")]
    public async Task<IActionResult> Reorder(Guid id)
    {
        try
        {
            var result = await _orderService.ReorderAsync(User.GetUserId(), id);
            return Ok(new ApiResponse { Success = true, Data = result });
        }
        catch (ApiException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest request)
    {
        try
        {
            await _orderService.CancelOrderAsync(User.GetUserId(), id, request.Reason);
            return Ok(new ApiResponse { Success = true, Message = "Order cancelled." });
        }
        catch (ApiException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
    }
}