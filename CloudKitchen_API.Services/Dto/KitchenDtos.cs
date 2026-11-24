using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Dto
{
    public class KitchenOrderListItemDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string PaymentMode { get; set; } = null!;
        public DateTime PlacedAt { get; set; }
        public int ItemCount { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerMobile { get; set; }
    }

    public class KitchenOrderDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? CouponCode { get; set; }
        public string PaymentMode { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public DateTime PlacedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }          
        public string? CancelReason { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerMobile { get; set; } = null!;
        public string AddressLine { get; set; } = null!;
        public string? RiderName { get; set; }
        public string? RiderMobile { get; set; }
        public List<KitchenOrderLineDto> Lines { get; set; } = new();
    }

    public class KitchenOrderLineDto
    {
        public string ItemName { get; set; } = null!;
        public string Portion { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = null!;
    }

    public class RejectOrderRequest
    {
        public string Reason { get; set; } = null!;
    }

    public class AssignRiderRequest
    {
        public Guid RiderId { get; set; }
    }
}
