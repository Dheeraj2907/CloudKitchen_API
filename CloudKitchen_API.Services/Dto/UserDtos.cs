namespace CloudKitchen_API.Services.Dto;

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
}

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string Mobile { get; set; } = null!;
    public string? Email { get; set; }
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class AddressDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public string Line1 { get; set; } = null!;
    public string? Line2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Pincode { get; set; } = null!;
    public string? Landmark { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
}

public class AddressCreateRequest
{
    public string Label { get; set; } = null!;
    public string Line1 { get; set; } = null!;
    public string? Line2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Pincode { get; set; } = null!;
    public string? Landmark { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsDefault { get; set; }
}