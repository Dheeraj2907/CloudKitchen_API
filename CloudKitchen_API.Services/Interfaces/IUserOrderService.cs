using CloudKitchen_API.Services.Dto;

namespace CloudKitchen_API.Services.Interfaces;

public interface IOrderService
{
    Task<PlaceOrderResponse> PlaceOrderAsync(Guid userId, PlaceOrderRequest request);
    Task<TrackOrderResponse> TrackOrderAsync(Guid userId, Guid orderId);
    Task<ReorderResponse> ReorderAsync(Guid userId, Guid orderId);
    Task CancelOrderAsync(Guid userId, Guid orderId, string reason);
}