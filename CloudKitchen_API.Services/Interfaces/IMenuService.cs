using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Interfaces
{
    public interface IUserMenuService
    {
        Task<List<MenuItemDto>> GetAvailableMenuAsync();
        Task<MenuItemDto?> GetItemDetailsAsync(int itemId);
        Task<List<CategoryDto>> GetCategoriesAsync();
    }
}
