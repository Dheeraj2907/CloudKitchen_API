using CloudKitchen_API.Services.Dto;

namespace CloudKitchen_API.Services.Interfaces;

public interface IRiderService
{
    Task<RiderDto?> GetRiderForOrderAsync(Guid userId, Guid orderId);
}