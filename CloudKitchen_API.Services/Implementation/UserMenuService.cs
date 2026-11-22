using AutoMapper;
using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation
{
    public class UserMenuService : IUserMenuService
    {
        private readonly KitchenDbContext _dbContext;
        private readonly IMapper _mapper;


        public UserMenuService(KitchenDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<MenuItemDto>> GetAvailableMenuAsync()
        {
            // User only sees items that are in stock
            var menuItems = await _dbContext.MenuItems
                .Where(m => m.IsAvailable == true)
                .ToListAsync();
            return _mapper.Map<List<MenuItemDto>>(menuItems);


        }

        public async Task<MenuItemDto?> GetItemDetailsAsync(Guid itemId)
        {
            var menuItem = await _dbContext.MenuItems.FirstOrDefaultAsync(m => m.Id == itemId);
            if(menuItem is null)
            {
                throw new ApiException("Item not found");
            }
            return _mapper.Map<MenuItemDto>(menuItem);
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _dbContext.Categories.ToListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);

        }
    }
}