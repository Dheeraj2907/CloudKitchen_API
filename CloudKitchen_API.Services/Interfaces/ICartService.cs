using CloudKitchen_API.Services.Dto;

namespace CloudKitchen_API.Services.Interfaces;

public interface ICartService
{
    Task<CartResponse> AddToCartAsync(Guid userId, AddToCartRequest request);
    Task<CartResponse> GetCartAsync(Guid userId);
    Task<CartResponse> UpdateQuantityAsync(Guid userId, Guid cartItemId, int quantity);
    Task RemoveItemAsync(Guid userId, Guid cartItemId);
}