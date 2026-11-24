using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudKitchen_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/menu")]
    [ApiController]
    public class AdminMenuController : ControllerBase
    {
        private readonly IAdminMenuService _svc;
        public AdminMenuController(IAdminMenuService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, [FromQuery] bool? available,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
            => Ok(new ApiResponse { Success = true, Data = await _svc.GetAllAsync(categoryId, available, page, pageSize) });

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] AdminMenuItemCreateRequest r)
        {
            try { return Ok(new ApiResponse { Success = true, Data = await _svc.CreateAsync(r) }); }
            catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] AdminMenuItemUpdateRequest r)
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
            catch (ApiException ex) { return NotFound(new ApiResponse { Success = false, Message = ex.Message }); }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> Categories()
            => Ok(new ApiResponse { Success = true, Data = await _svc.GetCategoriesAsync() });

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromForm] AdminCategoryRequest r)
        {
            try { return Ok(new ApiResponse { Success = true, Data = await _svc.CreateCategoryAsync(r) }); }
            catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        }

        [HttpPut("categories/{id:guid}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromForm] AdminCategoryRequest r)
        {
            try { return Ok(new ApiResponse { Success = true, Data = await _svc.UpdateCategoryAsync(id, r) }); }
            catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        }

        [HttpDelete("categories/{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try { await _svc.DeleteCategoryAsync(id); return Ok(new ApiResponse { Success = true }); }
            catch (ApiException ex) { return BadRequest(new ApiResponse { Success = false, Message = ex.Message }); }
        }
    }
}
