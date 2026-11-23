namespace CloudKitchen_API.Services.Dto;

public class NotificationListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string? Type { get; set; }
    public Guid? RefId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RegisterDeviceRequest
{
    public string Token { get; set; } = null!;
    public string Platform { get; set; } = null!;   // android | ios | web
}