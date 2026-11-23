using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloudKitchen_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    public MenuController(IMenuService menuService) => _menuService = menuService;

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(new ApiResponse { Success = true, Data = await _menuService.GetByCategoryAsync(categoryId, page, pageSize) });

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        try { return Ok(new ApiResponse { Success = true, Data = await _menuService.GetItemDetailsAsync(id) }); }
        catch (ApiException ex) { return NotFound(new ApiResponse { Success = false, Message = ex.Message }); }
    }

    [HttpGet("popular")]
    public async Task<IActionResult> Popular([FromQuery] int limit = 10)
        => Ok(new ApiResponse { Success = true, Data = await _menuService.GetPopularAsync(limit) });

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(new ApiResponse { Success = true, Data = await _menuService.SearchAsync(q, page, pageSize) });
}