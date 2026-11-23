namespace CloudKitchen_API.Services.Dto;

public class MenuItemListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? HalfPrice { get; set; }
    public decimal? FullPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPopular { get; set; }
}

public class MenuItemDetailDto : MenuItemListDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public bool IsAvailable { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<ReviewDto> RecentReviews { get; set; } = new();
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}