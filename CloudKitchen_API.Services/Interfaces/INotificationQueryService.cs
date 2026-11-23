using CloudKitchen_API.Services.Dto;
namespace CloudKitchen_API.Services.Interfaces;

public interface INotificationQueryService
{
    Task<List<NotificationListDto>> GetAsync(Guid userId, int page, int pageSize);
    Task MarkReadAsync(Guid userId, Guid id);
    Task MarkAllReadAsync(Guid userId);
    Task RegisterDeviceAsync(Guid userId, string token, string platform);
}