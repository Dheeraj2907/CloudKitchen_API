using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Hubs;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Implementation
{
    public class OrderNotifier : IOrderNotifier
    {
        private readonly IHubContext<OrderHub> _hub;
        private readonly INotificationService _push;

        public OrderNotifier(IHubContext<OrderHub> hub, INotificationService push)
        {
            _hub = hub;
            _push = push;
        }

        public async Task OrderPlacedAsync(PlaceOrderResponse r, int itemCount)
        {
            await _hub.Clients.Group("kitchen").SendAsync("NewOrder", new
            {
                r.OrderId,
                r.OrderNumber,
                r.Status,
                r.TotalAmount,
                ItemCount = itemCount
            });
        }

        public async Task OrderStatusChangedAsync(
            Guid orderId, string status, DateTime? cancelledAt,
            string? cancelReason, Guid userId, string orderNumber)
        {
            // 1. SignalR push to anyone watching this order live
            await _hub.Clients.Group($"order-{orderId}").SendAsync("OrderStatusChanged", new
            {
                OrderId = orderId,
                Status = status,
                CancelledAt = cancelledAt,
                CancelReason = cancelReason
            });

            // 2. FCM push to the customer's devices (fail-open — never breaks the status change)
            try
            {
                await _push.SendOrderStatusPushAsync(userId, orderId, orderNumber, status);
            }
            catch
            {
                // swallow; a push outage must not roll back the order status update
            }
        }
    }
}
