using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using static System.Net.Mime.MediaTypeNames;

namespace CloudKitchen_API.Services.Implementation
{
    public class AdminMenuService : IAdminMenuService
    {
        private readonly KitchenDbContext _db;
        public AdminMenuService(KitchenDbContext db, IImageStorageService storage)
        {
            _db = db;
            _storage = storage;
        }
        private readonly IImageStorageService _storage;

        public async Task<List<MenuItemDetailDto>> GetAllAsync(Guid? categoryId, bool? available, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var q = _db.MenuItems.Include(m => m.Category).AsQueryable();
            if (categoryId.HasValue) q = q.Where(m => m.CategoryId == categoryId);
            if (available.HasValue) q = q.Where(m => m.IsAvailable == available);

            var items = await q.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Name)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return items.Select(ToDetail).ToList();
        }

        public async Task<MenuItemDetailDto> CreateAsync(AdminMenuItemCreateRequest r)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == r.CategoryId))
                throw new ApiException("Category not found.");
            if (string.IsNullOrWhiteSpace(r.Name))
                throw new ApiException("Name required.");
            if (r.Price < 0) throw new ApiException("Price cannot be negative.");

            // Save the image first (if any)
            string? imageUrl = null;
            if (r.Image != null && !string.IsNullOrEmpty(r.Image.FileName))
                imageUrl = await _storage.SaveAsync(r.Image.OpenReadStream(), r.Image.FileName, r.Image.ContentType ?? "image/jpeg", "menu");

            var m = new MenuItem
            {
                Id = Guid.NewGuid(),
                CategoryId = r.CategoryId,
                Name = r.Name.Trim(),
                Description = r.Description?.Trim(),
                HalfPrice = r.HalfPrice,
                FullPrice = r.FullPrice,
                ImageUrl = imageUrl,
                IsAvailable = true,
                IsPopular = r.IsPopular,
                DisplayOrder = r.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };
            _db.MenuItems.Add(m);
            await _db.SaveChangesAsync();
            await _db.Entry(m).Reference(x => x.Category).LoadAsync();
            return ToDetail(m);
        }

        public async Task<MenuItemDetailDto> UpdateAsync(Guid id, AdminMenuItemUpdateRequest r)
        {
            var m = await _db.MenuItems.Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new ApiException("Menu item not found.");
            if (!await _db.Categories.AnyAsync(c => c.Id == r.CategoryId))
                throw new ApiException("Category not found.");

            var oldImage = m.ImageUrl;

            // If a new image was provided, save it; otherwise keep the old URL
            string? newImage = oldImage;
            if (r.Image != null && !string.IsNullOrEmpty(r.Image.FileName))
            {
                newImage = await _storage.SaveAsync(r.Image.OpenReadStream(), r.Image.FileName, r.Image.ContentType ?? "image/jpeg", "menu");
            }

            m.CategoryId = r.CategoryId; 
            m.Name = r.Name.Trim(); 
            m.Description = r.Description?.Trim();
            m.Price = r.Price; 
            m.HalfPrice = r.HalfPrice; 
            m.FullPrice = r.FullPrice;
            m.ImageUrl = r.ImageUrl; 
            m.IsPopular = r.IsPopular; 
            m.DisplayOrder = r.DisplayOrder;

            await _db.SaveChangesAsync();

            // Delete the old image only if it was replaced
            if (!string.IsNullOrWhiteSpace(oldImage) && oldImage != newImage)
                await _storage.DeleteAsync(oldImage);

            await _db.Entry(m).Reference(x => x.Category).LoadAsync();
            return ToDetail(m);
        }

        public async Task SetAvailabilityAsync(Guid id, bool isAvailable)
        {
            var m = await _db.MenuItems.FindAsync(id) ?? throw new ApiException("Menu item not found.");
            m.IsAvailable = isAvailable;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var m = await _db.MenuItems.FindAsync(id) ?? throw new ApiException("Menu item not found.");
            // Soft-delete: keep the row (order_items reference it), just hide it
            m.IsAvailable = false;
            await _db.SaveChangesAsync();
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            return await _db.Categories.OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.ImageUrl,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    ItemCount = c.MenuItems.Count
                }).ToListAsync();
        }

        public async Task<CategoryDto> CreateCategoryAsync(AdminCategoryRequest r)
        {
            // Save the image first (if any)
            string? imageUrl = null;
            if (r.Image != null && !string.IsNullOrEmpty(r.Image.FileName))
                imageUrl = await _storage.SaveAsync(r.Image.OpenReadStream(), r.Image.FileName, r.Image.ContentType ?? "image/jpeg", "menu");

            var c = new Category
            {
                Id = Guid.NewGuid(),
                Name = r.Name.Trim(),
                ImageUrl = imageUrl,
                DisplayOrder = r.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Categories.Add(c);
            await _db.SaveChangesAsync();
            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ItemCount = 0
            };
        }

        public async Task<CategoryDto> UpdateCategoryAsync(Guid id, AdminCategoryRequest r)
        {
            var c = await _db.Categories.FindAsync(id) ?? throw new ApiException("Category not found.");

            var oldImage = c.ImageUrl;

            // If a new image was provided, save it; otherwise keep the old URL
            string? newImage = oldImage;
            if (r.Image != null && !string.IsNullOrEmpty(r.Image.FileName))
            {
                newImage = await _storage.SaveAsync(r.Image.OpenReadStream(), r.Image.FileName, r.Image.ContentType ?? "image/jpeg", "menu");
            }

            c.Name = r.Name.Trim(); 
            c.ImageUrl = newImage; 
            c.DisplayOrder = r.DisplayOrder;

            await _db.SaveChangesAsync();

            // Delete the old image only if it was replaced
            if (!string.IsNullOrWhiteSpace(oldImage) && oldImage != newImage)
                await _storage.DeleteAsync(oldImage);

            var count = await _db.MenuItems.CountAsync(m => m.CategoryId == id);
            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ItemCount = count
            };
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var c = await _db.Categories.FindAsync(id) ?? throw new ApiException("Category not found.");
            if (await _db.MenuItems.AnyAsync(m => m.CategoryId == id))
                throw new ApiException("Cannot delete category with menu items. Move or delete them first.");
            _db.Categories.Remove(c);
            await _db.SaveChangesAsync();
        }

        private static MenuItemDetailDto ToDetail(MenuItem m) => new()
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            HalfPrice = m.HalfPrice,
            FullPrice = m.FullPrice,
            ImageUrl = m.ImageUrl,
            IsPopular = m.IsPopular,
            IsAvailable = m.IsAvailable,
            CategoryId = m.CategoryId,
            CategoryName = m.Category.Name
        };
    }
}