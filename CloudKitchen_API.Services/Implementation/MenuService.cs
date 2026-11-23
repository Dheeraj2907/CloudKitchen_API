using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation;

public class MenuService : IMenuService
{
    private readonly KitchenDbContext _db;

    public MenuService(KitchenDbContext db) => _db = db;

    public async Task<List<MenuItemListDto>> GetByCategoryAsync(Guid categoryId, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        return await _db.MenuItems
            .Where(m => m.CategoryId == categoryId && m.IsAvailable)
            .OrderBy(m => m.DisplayOrder).ThenBy(m => m.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new MenuItemListDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                HalfPrice = m.HalfPrice,
                FullPrice = m.FullPrice,
                ImageUrl = m.ImageUrl,
                IsPopular = m.IsPopular
            }).ToListAsync();
    }

    public async Task<MenuItemDetailDto> GetItemDetailsAsync(Guid itemId)
    {
        var item = await _db.MenuItems
            .Include(m => m.Category)
            .Include(m => m.Reviews).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == itemId)
            ?? throw new ApiException("Menu item not found.");

        var ratings = item.Reviews.Select(r => r.Rating).ToList();
        return new MenuItemDetailDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            HalfPrice = item.HalfPrice,
            FullPrice = item.FullPrice,
            ImageUrl = item.ImageUrl,
            IsPopular = item.IsPopular,
            IsAvailable = item.IsAvailable,
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            AverageRating = ratings.Count == 0 ? 0 : Math.Round(ratings.Average(), 1),
            ReviewCount = ratings.Count,
            RecentReviews = item.Reviews
                .OrderByDescending(r => r.CreatedAt).Take(5)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserName = r.User.FullName ?? "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
        };
    }

    public async Task<List<MenuItemListDto>> GetPopularAsync(int limit)
    {
        limit = Math.Clamp(limit, 1, 20);
        return await _db.MenuItems
            .Where(m => m.IsPopular && m.IsAvailable)
            .OrderBy(m => m.DisplayOrder)
            .Take(limit)
            .Select(m => new MenuItemListDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                HalfPrice = m.HalfPrice,
                FullPrice = m.FullPrice,
                ImageUrl = m.ImageUrl,
                IsPopular = m.IsPopular
            }).ToListAsync();
    }

    public async Task<List<MenuItemListDto>> SearchAsync(string query, int page, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(query)) return new();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var q = query.Trim().ToLower();

        return await _db.MenuItems
            .Where(m => m.IsAvailable &&
                        (m.Name.ToLower().Contains(q) ||
                         (m.Description != null && m.Description.ToLower().Contains(q))))
            .OrderBy(m => m.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new MenuItemListDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                HalfPrice = m.HalfPrice,
                FullPrice = m.FullPrice,
                ImageUrl = m.ImageUrl,
                IsPopular = m.IsPopular
            }).ToListAsync();
    }
}