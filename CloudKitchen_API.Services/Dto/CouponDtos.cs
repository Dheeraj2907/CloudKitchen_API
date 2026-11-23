namespace CloudKitchen_API.Services.Dto;

public class CouponListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscount { get; set; }
    public decimal MinOrderValue { get; set; }
    public DateTime ValidTo { get; set; }
}

public class ValidateCouponRequest
{
    public string Code { get; set; } = null!;
    public decimal OrderAmount { get; set; }
}

public class ValidateCouponResponse
{
    public bool IsValid { get; set; }
    public decimal Discount { get; set; }
    public string? Message { get; set; }
}