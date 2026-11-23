using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation;

public class NotificationQueryService : INotificationQueryService
{
    private readonly KitchenDbContext _db;
    public NotificationQueryService(KitchenDbContext db) => _db = db;

    public async Task<List<NotificationListDto>> GetAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new NotificationListDto
            {
                Id = n.Id,
                Title = n.Title,
                Body = n.Body,
                Type = n.Type,
                RefId = n.RefId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToListAsync();
    }

    public async Task MarkReadAsync(Guid userId, Guid id)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId)
            ?? throw new ApiException("Notification not found.");
        n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task RegisterDeviceAsync(Guid userId, string token, string platform)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ApiException("Token required.");
        if (platform is not ("android" or "ios" or "web"))
            throw new ApiException("Platform must be android, ios or web.");

        var existing = await _db.DeviceTokens.FirstOrDefaultAsync(d => d.Token == token);
        if (existing != null)
        {
            existing.UserId = userId;
            existing.Platform = platform;
        }
        else
        {
            _db.DeviceTokens.Add(new DeviceToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                Platform = platform,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();
    }
}