namespace CloudKitchen_API.Services.Interfaces;

public interface INotificationService
{
    Task SendOrderStatusPushAsync(Guid userId, Guid orderId, string orderNumber, string status);
    Task SendPushToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null);
}