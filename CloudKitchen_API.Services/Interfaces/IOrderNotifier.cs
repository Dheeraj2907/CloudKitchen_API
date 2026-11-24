using CloudKitchen_API.Services.Dto;

namespace CloudKitchen_API.Services.Interfaces
{
    public interface IOrderNotifier
    {
        Task OrderPlacedAsync(PlaceOrderResponse response, int itemCount);

        Task OrderStatusChangedAsync(
            Guid orderId,
            string status,
            DateTime? cancelledAt,
            string? cancelReason,
            Guid userId,
            string orderNumber);
    }
}
