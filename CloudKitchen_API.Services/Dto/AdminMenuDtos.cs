using Microsoft.AspNetCore.Http;

namespace CloudKitchen_API.Services.Dto;

public class AdminMenuItemCreateRequest
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? HalfPrice { get; set; }
    public decimal? FullPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPopular { get; set; }
    public int DisplayOrder { get; set; }
    public IFormFile? Image { get; set; }

}

public class AdminMenuItemUpdateRequest : AdminMenuItemCreateRequest { }

public class AdminCategoryRequest
{
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public IFormFile? Image { get; set; }
}

public class AvailabilityRequest
{
    public bool IsAvailable { get; set; }
}