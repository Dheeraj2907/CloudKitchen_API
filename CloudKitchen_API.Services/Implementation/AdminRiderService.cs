using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Implementation
{
    public class AdminRiderService : IAdminRiderService
    {
        private readonly KitchenDbContext _db;
        public AdminRiderService(KitchenDbContext db) => _db = db;

        public async Task<List<RiderAdminDto>> GetAllAsync()
            => _db.Riders.OrderBy(r => r.Name).Select(Map).ToList();

        public async Task<RiderAdminDto> CreateAsync(RiderCreateRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Name) || string.IsNullOrWhiteSpace(r.Mobile))
                throw new ApiException("Name and mobile are required.");
            var rider = new Rider
            {
                Id = Guid.NewGuid(),
                Name = r.Name.Trim(),
                Mobile = r.Mobile.Trim(),
                VehicleNumber = r.VehicleNumber?.Trim(),
                PhotoUrl = r.PhotoUrl,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Riders.Add(rider);
            await _db.SaveChangesAsync();
            return Map(rider);
        }

        public async Task<RiderAdminDto> UpdateAsync(Guid id, RiderCreateRequest r)
        {
            var rider = await _db.Riders.FindAsync(id) ?? throw new ApiException("Rider not found.");
            rider.Name = r.Name.Trim(); rider.Mobile = r.Mobile.Trim();
            rider.VehicleNumber = r.VehicleNumber?.Trim(); rider.PhotoUrl = r.PhotoUrl;
            await _db.SaveChangesAsync();
            return Map(rider);
        }

        public async Task SetAvailabilityAsync(Guid id, bool available)
        {
            var rider = await _db.Riders.FindAsync(id) ?? throw new ApiException("Rider not found.");
            rider.IsAvailable = available;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var rider = await _db.Riders.FindAsync(id) ?? throw new ApiException("Rider not found.");
            // FK on orders.rider_id prevents deletion if assigned to any order
            if (await _db.Orders.AnyAsync(o => o.RiderId == id))
                throw new ApiException("Rider is assigned to orders and cannot be deleted.");
            _db.Riders.Remove(rider);
            await _db.SaveChangesAsync();
        }

        private static RiderAdminDto Map(Rider r) => new()
        {
            Id = r.Id,
            Name = r.Name,
            Mobile = r.Mobile,
            VehicleNumber = r.VehicleNumber,
            PhotoUrl = r.PhotoUrl,
            IsAvailable = r.IsAvailable
        };
    }
}
