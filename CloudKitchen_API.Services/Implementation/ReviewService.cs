using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation;

public class ReviewService : IReviewService
{
    private readonly KitchenDbContext _db;
    public ReviewService(KitchenDbContext db) => _db = db;

    public async Task<ReviewDto> SubmitAsync(Guid userId, SubmitReviewRequest r)
    {
        if (r.Rating < 1 || r.Rating > 5) throw new ApiException("Rating must be 1–5.");

        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == r.OrderId && o.UserId == userId)
            ?? throw new ApiException("Order not found.");
        if (order.Status != "Delivered") throw new ApiException("You can only review delivered orders.");
        if (!order.OrderItems.Any(i => i.MenuItemId == r.MenuItemId))
            throw new ApiException("That item was not in this order.");

        var exists = await _db.Reviews.AnyAsync(rv =>
            rv.UserId == userId && rv.OrderId == r.OrderId && rv.MenuItemId == r.MenuItemId);
        if (exists) throw new ApiException("You already reviewed this item for this order.");

        var review = new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderId = r.OrderId,
            MenuItemId = r.MenuItemId,
            Rating = r.Rating,
            Comment = r.Comment?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        return new ReviewDto
        {
            Id = review.Id,
            UserName = user?.FullName ?? "Anonymous",
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<List<ReviewDto>> GetForItemAsync(Guid menuItemId, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        return await _db.Reviews.Include(r => r.User)
            .Where(r => r.MenuItemId == menuItemId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                UserName = r.User.FullName ?? "Anonymous",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToListAsync();
    }

    public async Task<List<MyReviewDto>> GetMineAsync(Guid userId)
    {
        return await _db.Reviews.Include(r => r.MenuItem)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MyReviewDto
            {
                Id = r.Id,
                OrderId = r.OrderId ?? Guid.Empty,
                MenuItemId = r.MenuItemId,
                ItemName = r.MenuItem.Name,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToListAsync();
    }
}