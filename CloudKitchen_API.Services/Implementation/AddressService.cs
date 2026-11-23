using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation;

public class AddressService : IAddressService
{
    private readonly KitchenDbContext _db;
    public AddressService(KitchenDbContext db) => _db = db;

    public async Task<List<AddressDto>> GetAsync(Guid userId)
    {
        return await _db.Addresses.Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt)
            .Select(a => Map(a)).ToListAsync();
    }

    public async Task<AddressDto> AddAsync(Guid userId, AddressCreateRequest r)
    {
        if (r.IsDefault)
            await _db.Addresses.Where(a => a.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));

        var addr = new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Label = r.Label,
            Line1 = r.Line1,
            Line2 = r.Line2,
            City = r.City,
            State = r.State,
            Pincode = r.Pincode,
            Landmark = r.Landmark,
            Latitude = r.Latitude,
            Longitude = r.Longitude,
            IsDefault = r.IsDefault,
            CreatedAt = DateTime.UtcNow
        };
        _db.Addresses.Add(addr);
        await _db.SaveChangesAsync();
        return Map(addr);
    }

    public async Task<AddressDto> UpdateAsync(Guid userId, Guid addressId, AddressCreateRequest r)
    {
        var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId)
            ?? throw new ApiException("Address not found.");

        if (r.IsDefault && !addr.IsDefault)
            await _db.Addresses.Where(a => a.UserId == userId && a.Id != addressId)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));

        addr.Label = r.Label; addr.Line1 = r.Line1; addr.Line2 = r.Line2;
        addr.City = r.City; addr.State = r.State; addr.Pincode = r.Pincode;
        addr.Landmark = r.Landmark; addr.Latitude = r.Latitude;
        addr.Longitude = r.Longitude; addr.IsDefault = r.IsDefault;

        await _db.SaveChangesAsync();
        return Map(addr);
    }

    public async Task DeleteAsync(Guid userId, Guid addressId)
    {
        var addr = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId)
            ?? throw new ApiException("Address not found.");

        // Refuse to delete an address referenced by any order
        var inUse = await _db.Orders.AnyAsync(o => o.AddressId == addressId);
        if (inUse) throw new ApiException("Address used by an order cannot be deleted.");

        _db.Addresses.Remove(addr);
        await _db.SaveChangesAsync();
    }

    public async Task SetDefaultAsync(Guid userId, Guid addressId)
    {
        if (!await _db.Addresses.AnyAsync(a => a.Id == addressId && a.UserId == userId))
            throw new ApiException("Address not found.");

        await _db.Addresses.Where(a => a.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, a => a.Id == addressId));
    }

    private static AddressDto Map(Address a) => new()
    {
        Id = a.Id,
        Label = a.Label,
        Line1 = a.Line1,
        Line2 = a.Line2,
        City = a.City,
        State = a.State,
        Pincode = a.Pincode,
        Landmark = a.Landmark,
        Latitude = a.Latitude,
        Longitude = a.Longitude,
        IsDefault = a.IsDefault
    };
}