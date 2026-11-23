namespace CloudKitchen_API.Services.Dto;

public class PlaceOrderRequest
{
    public Guid AddressId { get; set; }
    public string? CouponCode { get; set; }
    public string PaymentMode { get; set; } = "Cash";
}

public class PlaceOrderResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
}

public class TrackOrderResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime PlacedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public RiderDto? Rider { get; set; }
}

public class RiderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Mobile { get; set; } = null!;
    public string? VehicleNumber { get; set; }
    public string? PhotoUrl { get; set; }
}

public class CancelOrderRequest
{
    public string Reason { get; set; } = null!;
}

public class ReorderResponse
{
    public List<Guid> AddedMenuItemIds { get; set; } = new();
    public List<SkippedItem> Skipped { get; set; } = new();
}

public class SkippedItem
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = null!;
    public string Reason { get; set; } = null!;
}