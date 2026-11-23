namespace CloudKitchen_API.Services.Dto;

public class AddToCartRequest
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public string Portion { get; set; } = "Full";   // Half | Full
}

public class UpdateCartRequest
{
    public int Quantity { get; set; }
}

public class CartResponse
{
    public List<CartLineDto> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public int TotalQuantity { get; set; }
}

public class CartLineDto
{
    public Guid CartItemId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string Portion { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public bool IsAvailable { get; set; }
}