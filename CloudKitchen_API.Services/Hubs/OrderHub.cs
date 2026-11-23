using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CloudKitchen_API.Services.Hubs;

[Authorize]
public class OrderHub : Hub
{
    // Customer app calls this after placing an order to receive status updates
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    // Kitchen app calls this once on connect to receive new-order notifications
    [Authorize(Roles = "Kitchen,Admin")]
    public async Task JoinKitchenGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "kitchen");
    }
}