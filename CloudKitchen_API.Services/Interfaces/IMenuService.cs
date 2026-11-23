using CloudKitchen_API.Services.Dto;

namespace CloudKitchen_API.Services.Interfaces;

public interface IMenuService
{
    Task<List<MenuItemListDto>> GetByCategoryAsync(Guid categoryId, int page, int pageSize);
    Task<MenuItemDetailDto> GetItemDetailsAsync(Guid itemId);
    Task<List<MenuItemListDto>> GetPopularAsync(int limit);
    Task<List<MenuItemListDto>> SearchAsync(string query, int page, int pageSize);
}