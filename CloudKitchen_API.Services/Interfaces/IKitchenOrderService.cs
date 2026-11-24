using CloudKitchen_API.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Interfaces
{
    public interface IKitchenOrderService
    {
        Task<List<KitchenOrderListItemDto>> GetQueueAsync(string? status);
        Task<KitchenOrderDetailDto> GetDetailAsync(Guid id);
        Task<KitchenOrderDetailDto> UpdateStatusAsync(Guid id, string newStatus);
        Task<KitchenOrderDetailDto> RejectAsync(Guid id, string reason);
        Task AssignRiderAsync(Guid orderId, Guid riderId);
    }
}
