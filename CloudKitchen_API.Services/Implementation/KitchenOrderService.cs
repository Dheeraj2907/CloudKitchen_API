using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Implementation
{
    public class KitchenOrderService : IKitchenOrderService
    {
        private static readonly string[] AllowedTransitions = { "Placed", "Accepted", "Preparing", "Ready", "PickedUp", "Delivered" };

        private readonly KitchenDbContext _db;
        public KitchenOrderService(KitchenDbContext db) => _db = db;

        public async Task<List<KitchenOrderListItemDto>> GetQueueAsync(string? status)
        {
            var q = _db.Orders.Include(o => o.User).Include(o => o.OrderItems).AsQueryable();

            if (string.IsNullOrEmpty(status))
                q = q.Where(o => o.Status != "Delivered" && o.Status != "Cancelled" && o.Status != "Rejected");
            else
                q = q.Where(o => o.Status == status);

            return await q.OrderBy(o => o.PlacedAt)
                .Select(o => new KitchenOrderListItemDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    PaymentMode = o.PaymentMode,
                    PlacedAt = o.PlacedAt,
                    ItemCount = o.OrderItems.Sum(i => i.Quantity),
                    CustomerName = o.User.FullName,
                    CustomerMobile = o.User.Mobile
                }).ToListAsync();
        }

        public async Task<KitchenOrderDetailDto> GetDetailAsync(Guid id)
        {
            var o = await _db.Orders
                .Include(x => x.User).Include(x => x.Address).Include(x => x.Rider)
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new ApiException("Order not found.");
            return Map(o);
        }

        public async Task<KitchenOrderDetailDto> UpdateStatusAsync(Guid id, string newStatus)
        {
            if (!AllowedTransitions.Contains(newStatus))
                throw new ApiException($"Invalid status '{newStatus}'.");

            var o = await _db.Orders
                .Include(x => x.User).Include(x => x.Address).Include(x => x.Rider)
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new ApiException("Order not found.");

            if (o.Status is "Cancelled" or "Rejected" or "Delivered")
                throw new ApiException($"Order already {o.Status}; cannot update.");

            // Simple forward-only guard. Adjust if you need skip transitions (e.g. Placed → Preparing).
            var currentIdx = Array.IndexOf(AllowedTransitions, o.Status);
            var nextIdx = Array.IndexOf(AllowedTransitions, newStatus);
            if (nextIdx < currentIdx)
                throw new ApiException($"Cannot move back from '{o.Status}' to '{newStatus}'.");

            o.Status = newStatus;
            if (newStatus == "Delivered") o.DeliveredAt = DateTime.UtcNow;

            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                throw new ApiException("Order was updated by another station. Refresh and try again.");
            }

            await _db.Entry(o).Reference(x => x.User).LoadAsync();
            return Map(o);
        }

        public async Task<KitchenOrderDetailDto> RejectAsync(Guid id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new ApiException("Rejection reason required.");

            var o = await _db.Orders
                .Include(x => x.User).Include(x => x.Address).Include(x => x.Rider)
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new ApiException("Order not found.");

            if (o.Status is not ("Placed" or "Accepted"))
                throw new ApiException($"Cannot reject in '{o.Status}' state.");

            o.Status = "Rejected";
            o.CancelReason = reason.Trim();
            o.CancelledAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(o.CouponCode))
            {
                var c = await _db.Coupons.FirstOrDefaultAsync(x => x.Code == o.CouponCode);
                if (c != null && c.UsedCount > 0) c.UsedCount -= 1;
            }

            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { throw new ApiException("Order was updated elsewhere."); }

            return Map(o);
        }

        public async Task AssignRiderAsync(Guid orderId, Guid riderId)
        {
            var o = await _db.Orders.FirstOrDefaultAsync(x => x.Id == orderId)
                ?? throw new ApiException("Order not found.");
            if (!await _db.Riders.AnyAsync(r => r.Id == riderId && r.IsAvailable))
                throw new ApiException("Rider not found or unavailable.");

            o.RiderId = riderId;
            await _db.SaveChangesAsync();
        }

        private static KitchenOrderDetailDto Map(Order o) => new()
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status,
            SubTotal = o.SubTotal,
            DeliveryFee = o.DeliveryFee,
            Discount = o.Discount,
            TotalAmount = o.TotalAmount,
            CouponCode = o.CouponCode,
            PaymentMode = o.PaymentMode,
            PaymentStatus = o.PaymentStatus,
            PlacedAt = o.PlacedAt,
            DeliveredAt = o.DeliveredAt,
            CancelReason = o.CancelReason,
            CancelledAt = o.CancelledAt,       
            CustomerName = o.User.FullName ?? "—",
            CustomerMobile = o.User.Mobile,
            AddressLine = $"{o.Address.Line1}, {o.Address.City} {o.Address.Pincode}",
            RiderName = o.Rider?.Name,
            RiderMobile = o.Rider?.Mobile,
            Lines = o.OrderItems.Select(i => new KitchenOrderLineDto
            {
                ItemName = i.ItemName,
                Portion = i.Portion,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }
}
