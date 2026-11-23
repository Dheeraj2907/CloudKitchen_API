using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Hubs;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CloudKitchen_API.Services;

public class OrderService : IOrderService
{
    private readonly KitchenDbContext _db;
    private readonly IHubContext<OrderHub> _hub;
    private readonly IConfiguration _config;

    public OrderService(KitchenDbContext db, IHubContext<OrderHub> hub, IConfiguration config)
    {
        _db = db;
        _hub = hub;
        _config = config;
    }

    public async Task<PlaceOrderResponse> PlaceOrderAsync(Guid userId, PlaceOrderRequest request)
    {
        if (request.PaymentMode is not ("Cash" or "Online"))
            throw new ApiException("Invalid payment mode.");

        // Validate address belongs to user
        var address = await _db.Addresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId)
            ?? throw new ApiException("Address not found.");

        // Load cart
        var cartItems = await _db.CartItems
            .Include(c => c.MenuItem)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
            throw new ApiException("Cart is empty.");

        // Validate availability and compute totals with price snapshot
        var orderItems = new List<OrderItem>();
        decimal subTotal = 0;

        foreach (var ci in cartItems)
        {
            var mi = ci.MenuItem;
            if (!mi.IsAvailable)
                throw new ApiException($"'{mi.Name}' is no longer available.");

            //TODO
            // Portion-aware price
            decimal unitPrice;
                
                //ci.Portion == "Half"
                //? mi.HalfPrice ?? throw new ApiException($"Half portion not available for '{mi.Name}'.")
                //: mi.FullPrice ?? mi.Price;

            var lineTotal = 1 * ci.Quantity;
            subTotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                MenuItemId = mi.Id,
                ItemName = mi.Name,
                Quantity = ci.Quantity,
                LineTotal = lineTotal,
            });
        }

        // Coupon
        decimal discount = 0;
        Coupon? appliedCoupon = null;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            appliedCoupon = await _db.Coupons
                .FirstOrDefaultAsync(c => c.Code == request.CouponCode.Trim().ToUpper() && c.IsActive);

            if (appliedCoupon == null)
                throw new ApiException("Invalid coupon code.");
            if (DateTime.UtcNow < appliedCoupon.ValidFrom || DateTime.UtcNow > appliedCoupon.ValidTo)
                throw new ApiException("Coupon has expired.");
            if (subTotal < appliedCoupon.MinOrderValue)
                throw new ApiException($"Minimum order value for this coupon is {appliedCoupon.MinOrderValue}.");
            if (appliedCoupon.UsageLimit.HasValue && appliedCoupon.UsedCount >= appliedCoupon.UsageLimit.Value)
                throw new ApiException("Coupon usage limit reached.");

            discount = appliedCoupon.DiscountType == "Percent"
                ? Math.Round(subTotal * appliedCoupon.DiscountValue / 100m, 2)
                : appliedCoupon.DiscountValue;

            if (appliedCoupon.MaxDiscount.HasValue)
                discount = Math.Min(discount, appliedCoupon.MaxDiscount.Value);

            discount = Math.Min(discount, subTotal);
        }

        var deliveryFee = _config.GetValue<decimal?>("OrderSettings:DeliveryFee") ?? 0m;
        var totalAmount = Math.Max(0, subTotal - discount) + deliveryFee;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            AddressId = address.Id,
            Status = "Placed",
            SubTotal = subTotal,
            DeliveryFee = deliveryFee,
            Discount = discount,
            TotalAmount = totalAmount,
            CouponCode = appliedCoupon?.Code,
            PaymentMode = request.PaymentMode,
            PaymentStatus = "Pending",
            PlacedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Orders.Add(order);
            if (appliedCoupon != null) appliedCoupon.UsedCount += 1;
            _db.CartItems.RemoveRange(cartItems);   // clear cart
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        // Push to kitchen
        await _hub.Clients.Group("kitchen").SendAsync("NewOrder", new
        {
            order.Id,
            order.OrderNumber,
            order.Status,
            order.TotalAmount,
            order.PlacedAt,
            ItemCount = orderItems.Sum(i => i.Quantity)
        });

        return new PlaceOrderResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            SubTotal = subTotal,
            DeliveryFee = deliveryFee,
            Discount = discount,
            TotalAmount = totalAmount,
            Status = order.Status
        };
    }

    public async Task<TrackOrderResponse> TrackOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Rider)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new ApiException("Order not found.");

        return new TrackOrderResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            PlacedAt = order.PlacedAt,
            DeliveredAt = order.DeliveredAt,
            Rider = order.Rider == null ? null : new RiderDto
            {
                Id = order.Rider.Id,
                Name = order.Rider.Name,
                Mobile = order.Rider.Mobile,
                VehicleNumber = order.Rider.VehicleNumber,
                PhotoUrl = order.Rider.PhotoUrl
            }
        };
    }

    public async Task<ReorderResponse> ReorderAsync(Guid userId, Guid orderId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new ApiException("Order not found.");

        var response = new ReorderResponse();

        foreach (var item in order.OrderItems)
        {
            var mi = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
            if (mi == null)
            {
                response.Skipped.Add(new SkippedItem { MenuItemId = item.MenuItemId, Name = item.ItemName, Reason = "Item no longer exists." });
                continue;
            }
            if (!mi.IsAvailable)
            {
                response.Skipped.Add(new SkippedItem { MenuItemId = mi.Id, Name = mi.Name, Reason = "Item is unavailable." });
                continue;
            }

            var existing = await _db.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.MenuItemId == mi.Id );

            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                _db.CartItems.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    MenuItemId = mi.Id,
                    Quantity = item.Quantity,
                    AddedAt = DateTime.UtcNow
                });
            }
            response.AddedMenuItemIds.Add(mi.Id);
        }

        await _db.SaveChangesAsync();
        return response;
    }

    public async Task CancelOrderAsync(Guid userId, Guid orderId, string reason)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new ApiException("Order not found.");

        if (order.Status is not ("Placed" or "Accepted"))
            throw new ApiException($"Order cannot be cancelled in '{order.Status}' state.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            order.Status = "Cancelled";
            order.CancelReason = reason;
            order.CancelledAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(order.CouponCode))
            {
                var c = await _db.Coupons.FirstOrDefaultAsync(x => x.Code == order.CouponCode);
                if (c != null && c.UsedCount > 0) c.UsedCount -= 1;
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            throw new ApiException("Order was updated by kitchen. Refresh and try again.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await _hub.Clients.Group($"order-{orderId}").SendAsync("OrderStatusChanged", new
        {
            order.Id,
            order.Status,
            order.CancelledAt,
            order.CancelReason
        });
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
}