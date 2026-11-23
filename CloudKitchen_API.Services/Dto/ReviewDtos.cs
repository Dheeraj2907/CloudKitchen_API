namespace CloudKitchen_API.Services.Dto;

public class SubmitReviewRequest
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class MyReviewDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}