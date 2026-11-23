using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation;

public class UserService : IUserService
{
    private readonly KitchenDbContext _db;
    public UserService(KitchenDbContext db) => _db = db;

    public async Task<ProfileResponse> GetProfileAsync(Guid userId)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId)
            ?? throw new ApiException("User not found.");
        return Map(u);
    }

    public async Task<ProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId)
            ?? throw new ApiException("User not found.");
        if (request.FullName != null) u.FullName = request.FullName.Trim();
        if (request.Email != null) u.Email = request.Email.Trim();
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(u);
    }

    private static ProfileResponse Map(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Mobile = u.Mobile,
        Email = u.Email,
        Role = u.Role,
        CreatedAt = u.CreatedAt
    };
}