using CloudKitchen_API.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Interfaces
{
    public interface IAdminMenuService
    {
        Task<List<MenuItemDetailDto>> GetAllAsync(Guid? categoryId, bool? available, int page, int pageSize);
        Task<MenuItemDetailDto> CreateAsync(AdminMenuItemCreateRequest request);
        Task<MenuItemDetailDto> UpdateAsync(Guid id, AdminMenuItemUpdateRequest request);
        Task SetAvailabilityAsync(Guid id, bool isAvailable);
        Task DeleteAsync(Guid id);

        Task<List<CategoryDto>> GetCategoriesAsync();
        Task<CategoryDto> CreateCategoryAsync(AdminCategoryRequest request);
        Task<CategoryDto> UpdateCategoryAsync(Guid id, AdminCategoryRequest request);
        Task DeleteCategoryAsync(Guid id);
    }
}
