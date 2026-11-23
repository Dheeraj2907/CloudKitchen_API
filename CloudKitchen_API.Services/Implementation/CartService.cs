using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation;

public class CartService : ICartService
{
    private readonly KitchenDbContext _db;
    public CartService(KitchenDbContext db) => _db = db;

    public async Task<CartResponse> AddToCartAsync(Guid userId, AddToCartRequest request)
    {
        if (request.Quantity < 1) throw new ApiException("Quantity must be at least 1.");
        if (request.Portion is not ("Half" or "Full")) throw new ApiException("Portion must be 'Half' or 'Full'.");

        var mi = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == request.MenuItemId)
            ?? throw new ApiException("Menu item not found.");
        if (!mi.IsAvailable) throw new ApiException($"'{mi.Name}' is unavailable.");
        if (request.Portion == "Half" && mi.HalfPrice == null)
            throw new ApiException($"Half portion not available for '{mi.Name}'.");

        var existing = await _db.CartItems.FirstOrDefaultAsync(c =>
            c.UserId == userId && c.MenuItemId == mi.Id && c.Portion == request.Portion);

        if (existing != null) existing.Quantity += request.Quantity;
        else _db.CartItems.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MenuItemId = mi.Id,
            Quantity = request.Quantity,
            Portion = request.Portion,
            AddedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartResponse> GetCartAsync(Guid userId)
    {
        var items = await _db.CartItems.Include(c => c.MenuItem)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.AddedAt)
            .ToListAsync();

        var lines = items.Select(c =>
        {
            decimal unit = c.Portion == "Half"
                ? c.MenuItem.HalfPrice ?? 0
                : c.MenuItem.FullPrice ?? c.MenuItem.Price;
            return new CartLineDto
            {
                CartItemId = c.Id,
                MenuItemId = c.MenuItemId,
                Name = c.MenuItem.Name,
                ImageUrl = c.MenuItem.ImageUrl,
                Portion = c.Portion,
                UnitPrice = unit,
                Quantity = c.Quantity,
                LineTotal = unit * c.Quantity,
                IsAvailable = c.MenuItem.IsAvailable
            };
        }).ToList();

        return new CartResponse
        {
            Items = lines,
            SubTotal = lines.Sum(l => l.LineTotal),
            TotalQuantity = lines.Sum(l => l.Quantity)
        };
    }

    public async Task<CartResponse> UpdateQuantityAsync(Guid userId, Guid cartItemId, int quantity)
    {
        if (quantity < 1) throw new ApiException("Quantity must be at least 1.");
        var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId)
            ?? throw new ApiException("Cart item not found.");
        item.Quantity = quantity;
        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task RemoveItemAsync(Guid userId, Guid cartItemId)
    {
        var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId)
            ?? throw new ApiException("Cart item not found.");
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }
}