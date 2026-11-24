using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudKitchen_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/riders")]
    [ApiController]
    public class AdminRiderController : ControllerBase
    {
        private readonly IAdminRiderService _svc;
        public AdminRiderController(IAdminRiderService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(new ApiResponse { Success = true, Data = await _svc.GetAllAsync() });

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RiderCreateRequest r)
        {
            try { return Ok(new ApiResponse { Success = true, Data = await _svc.CreateAsync(r) }); }
            catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RiderCreateRequest r)
        {
            try { return Ok(new ApiResponse { Success = true, Data = await _svc.UpdateAsync(id, r) }); }
            catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        }

        [HttpPut("{id:guid}/availability")]
        public async Task<IActionResult> SetAvailability(Guid id, [FromBody] AvailabilityRequest r)
        {
            try { await _svc.SetAvailabilityAsync(id, r.IsAvailable); return Ok(new ApiResponse { Success = true }); }
            catch (ApiException ex) { return NotFound(new ApiResponse { Success = false, Message = ex.Message }); }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try { await _svc.DeleteAsync(id); return Ok(new ApiResponse { Success = true }); }
            catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        }
    }
}
