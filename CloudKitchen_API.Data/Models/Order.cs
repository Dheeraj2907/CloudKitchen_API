using System;
using System.Collections.Generic;

namespace CloudKitchen_API.Data.Models;

public partial class Order
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = null!;

    public Guid UserId { get; set; }

    public Guid AddressId { get; set; }

    public Guid? RiderId { get; set; }

    public string Status { get; set; } = null!;

    public decimal SubTotal { get; set; }

    public decimal DeliveryFee { get; set; }

    public decimal Discount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? CouponCode { get; set; }

    public string PaymentMode { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? CancelReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime PlacedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Rider? Rider { get; set; }

    public virtual User User { get; set; } = null!;
}
