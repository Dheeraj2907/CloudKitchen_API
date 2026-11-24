using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CloudKitchen_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/orders")]
    [ApiController]
    public class AdminOrderController : ControllerBase
    {
        private readonly IKitchenOrderService _svc;
        private readonly IOrderNotifier _notifier;

        public AdminOrderController(IKitchenOrderService svc, IOrderNotifier notifier)
        {
            _svc = svc;
            _notifier = notifier;
        }

        [HttpGet]
        public async Task<IActionResult> Queue([FromQuery] string? status)
            => Ok(new ApiResponse
            {
                Success = true,
                Data = await _svc.GetQueueAsync(status)
            });

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            try
            {
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = await _svc.GetDetailAsync(id)
                });
            }
            catch (ApiException ex)
            {
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest r)
        {
            try
            {
                var dto = await _svc.UpdateStatusAsync(id, r.Status);
                await _notifier.OrderStatusChangedAsync(
                    dto.Id, dto.Status, dto.CancelledAt, dto.CancelReason,
                    dto.UserId, dto.OrderNumber);
                return Ok(new ApiResponse { Success = true, Data = dto });
            }
            catch (ApiException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectOrderRequest r)
        {
            try
            {
                var dto = await _svc.RejectAsync(id, r.Reason);
                await _notifier.OrderStatusChangedAsync(
                    dto.Id, dto.Status, dto.CancelledAt, dto.CancelReason,
                    dto.UserId, dto.OrderNumber);
                return Ok(new ApiResponse { Success = true, Data = dto });
            }
            catch (ApiException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/rider")]
        public async Task<IActionResult> AssignRider(Guid id, [FromBody] AssignRiderRequest r)
        {
            try
            {
                await _svc.AssignRiderAsync(id, r.RiderId);
                return Ok(new ApiResponse { Success = true });
            }
            catch (ApiException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }
    }
}
