using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Implementation
{
    public class AdminCouponService : IAdminCouponService
    {
        private readonly KitchenDbContext _db;
        public AdminCouponService(KitchenDbContext db) => _db = db;

        public async Task<List<CouponListDto>> GetAllAsync(bool includeInactive)
        {
            var q = _db.Coupons.AsQueryable();
            if (!includeInactive) q = q.Where(c => c.IsActive);
            return await q.OrderByDescending(c => c.ValidTo).Select(c => new CouponListDto
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MaxDiscount = c.MaxDiscount,
                MinOrderValue = c.MinOrderValue,
                ValidTo = c.ValidTo
            }).ToListAsync();
        }

        public async Task<CouponListDto> CreateAsync(AdminCouponRequest r)
        {
            Validate(r);
            var code = r.Code.Trim().ToUpper();
            if (await _db.Coupons.AnyAsync(c => c.Code == code))
                throw new ApiException("Coupon code already exists.");

            var c = new Coupon
            {
                Id = Guid.NewGuid(),
                Code = code,
                Description = r.Description?.Trim(),
                DiscountType = r.DiscountType,
                DiscountValue = r.DiscountValue,
                MaxDiscount = r.MaxDiscount,
                MinOrderValue = r.MinOrderValue,
                ValidFrom = r.ValidFrom,
                ValidTo = r.ValidTo,
                UsageLimit = r.UsageLimit,
                UsedCount = 0,
                IsActive = r.IsActive
            };
            _db.Coupons.Add(c);
            await _db.SaveChangesAsync();
            return Map(c);
        }

        public async Task<CouponListDto> UpdateAsync(Guid id, AdminCouponRequest r)
        {
            Validate(r);
            var c = await _db.Coupons.FindAsync(id) ?? throw new ApiException("Coupon not found.");
            var code = r.Code.Trim().ToUpper();
            if (await _db.Coupons.AnyAsync(x => x.Code == code && x.Id != id))
                throw new ApiException("Coupon code already used by another coupon.");

            c.Code = code; c.Description = r.Description?.Trim();
            c.DiscountType = r.DiscountType; c.DiscountValue = r.DiscountValue;
            c.MaxDiscount = r.MaxDiscount; c.MinOrderValue = r.MinOrderValue;
            c.ValidFrom = r.ValidFrom; c.ValidTo = r.ValidTo;
            c.UsageLimit = r.UsageLimit; c.IsActive = r.IsActive;
            await _db.SaveChangesAsync();
            return Map(c);
        }

        public async Task SetActiveAsync(Guid id, bool active)
        {
            var c = await _db.Coupons.FindAsync(id) ?? throw new ApiException("Coupon not found.");
            c.IsActive = active;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var c = await _db.Coupons.FindAsync(id) ?? throw new ApiException("Coupon not found.");
            // Orders store a coupon_code string, so hard delete is safe here
            _db.Coupons.Remove(c);
            await _db.SaveChangesAsync();
        }

        private static void Validate(AdminCouponRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Code)) throw new ApiException("Code required.");
            if (r.DiscountType is not ("Flat" or "Percent")) throw new ApiException("DiscountType must be Flat or Percent.");
            if (r.DiscountValue <= 0) throw new ApiException("Discount value must be positive.");
            if (r.DiscountType == "Percent" && r.DiscountValue > 100) throw new ApiException("Percent discount cannot exceed 100.");
            if (r.ValidTo <= r.ValidFrom) throw new ApiException("ValidTo must be after ValidFrom.");
            if (r.UsageLimit is <= 0) throw new ApiException("UsageLimit must be positive if set.");
        }

        private static CouponListDto Map(Coupon c) => new()
        {
            Id = c.Id,
            Code = c.Code,
            Description = c.Description,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            MaxDiscount = c.MaxDiscount,
            MinOrderValue = c.MinOrderValue,
            ValidTo = c.ValidTo
        };
    }
}
