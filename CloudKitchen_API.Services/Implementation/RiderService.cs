using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation;

public class RiderService : IRiderService
{
    private readonly KitchenDbContext _db;
    public RiderService(KitchenDbContext db) => _db = db;

    /// Returns null when the order has no rider yet.
    /// Throws ApiException when the order doesn't exist or isn't owned by the user.
    public async Task<RiderDto?> GetRiderForOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Rider)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new ApiException("Order not found.");

        if (order.Rider == null) return null;

        return new RiderDto
        {
            Id = order.Rider.Id,
            Name = order.Rider.Name,
            Mobile = order.Rider.Mobile,
            VehicleNumber = order.Rider.VehicleNumber,
            PhotoUrl = order.Rider.PhotoUrl
        };
    }
}