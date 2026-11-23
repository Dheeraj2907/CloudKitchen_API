using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Services.Implementation;

public class CouponService : ICouponService
{
    private readonly KitchenDbContext _db;
    public CouponService(KitchenDbContext db) => _db = db;

    public async Task<List<CouponListDto>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;
        return await _db.Coupons
            .Where(c => c.IsActive && c.ValidFrom <= now && c.ValidTo >= now
                        && (c.UsageLimit == null || c.UsedCount < c.UsageLimit))
            .OrderBy(c => c.ValidTo)
            .Select(c => new CouponListDto
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

    public async Task<ValidateCouponResponse> ValidateAsync(ValidateCouponRequest r)
    {
        var now = DateTime.UtcNow;
        var c = await _db.Coupons.FirstOrDefaultAsync(x => x.Code == r.Code.Trim().ToUpper());
        if (c == null || !c.IsActive) return Invalid("Invalid coupon code.");
        if (now < c.ValidFrom || now > c.ValidTo) return Invalid("Coupon has expired.");
        if (c.UsageLimit != null && c.UsedCount >= c.UsageLimit) return Invalid("Coupon usage limit reached.");
        if (r.OrderAmount < c.MinOrderValue) return Invalid($"Minimum order of {c.MinOrderValue} required.");

        var discount = c.DiscountType == "Percent"
            ? Math.Round(r.OrderAmount * c.DiscountValue / 100m, 2)
            : c.DiscountValue;
        if (c.MaxDiscount != null) discount = Math.Min(discount, c.MaxDiscount.Value);
        discount = Math.Min(discount, r.OrderAmount);

        return new ValidateCouponResponse { IsValid = true, Discount = discount };
    }

    private static ValidateCouponResponse Invalid(string msg)
        => new() { IsValid = false, Discount = 0, Message = msg };
}