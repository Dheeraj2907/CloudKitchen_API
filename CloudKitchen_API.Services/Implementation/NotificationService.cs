using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CloudKitchen_API.Services.Implementation;

public class NotificationService : INotificationService
{
    private readonly KitchenDbContext _db;
    private readonly IConfiguration _config;

    public NotificationService(KitchenDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    private FirebaseMessaging GetMessaging()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            var path = _config["Firebase:ServiceAccountPath"]
                ?? throw new InvalidOperationException("Firebase:ServiceAccountPath not configured.");

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(path)
            });
        }
        return FirebaseMessaging.DefaultInstance;
    }

    public async Task SendOrderStatusPushAsync(Guid userId, Guid orderId, string orderNumber, string status)
    {
        var (title, body) = status switch
        {
            "Accepted" => ("Order Accepted", $"Your order {orderNumber} is being prepared."),
            "Preparing" => ("Order in Kitchen", $"Your order {orderNumber} is being cooked."),
            "Ready" => ("Order Ready", $"Your order {orderNumber} is ready for pickup."),
            "PickedUp" => ("Out for Delivery", $"Your order {orderNumber} is on the way."),
            "Delivered" => ("Order Delivered", $"Enjoy your meal! Order {orderNumber} has been delivered."),
            "Cancelled" => ("Order Cancelled", $"Your order {orderNumber} was cancelled."),
            "Rejected" => ("Order Rejected", $"Sorry, your order {orderNumber} could not be accepted."),
            _ => (null, null)
        };

        if (title == null) return;

        await SendPushToUserAsync(userId, title, body, new Dictionary<string, string>
        {
            { "type", "OrderUpdate" },
            { "orderId", orderId.ToString() },
            { "status", status }
        });
    }

    public async Task SendPushToUserAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null)
    {
        var tokens = await _db.DeviceTokens
            .Where(d => d.UserId == userId)
            .Select(d => d.Token)
            .ToListAsync();

        if (tokens.Count == 0) return;

        var messaging = GetMessaging();

        foreach (var batch in tokens.Chunk(500))
        {
            var message = new MulticastMessage
            {
                Tokens = batch.ToList(),
                Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
                Data = data ?? new Dictionary<string, string>()
            };

            var response = await messaging.SendEachForMulticastAsync(message);

            if (response.FailureCount > 0)
            {
                var deadTokens = new List<string>();
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    var r = response.Responses[i];
                    if (!r.IsSuccess && r.Exception?.MessagingErrorCode
                        is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
                    {
                        deadTokens.Add(batch.ElementAt(i));
                    }
                }

                if (deadTokens.Count > 0)
                {
                    var toRemove = await _db.DeviceTokens
                        .Where(d => deadTokens.Contains(d.Token))
                        .ToListAsync();
                    _db.DeviceTokens.RemoveRange(toRemove);
                    await _db.SaveChangesAsync();
                }
            }
        }
    }
}