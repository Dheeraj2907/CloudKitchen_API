using CloudKitchen_API.Services.Dto;
namespace CloudKitchen_API.Services.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> SubmitAsync(Guid userId, SubmitReviewRequest request);
    Task<List<ReviewDto>> GetForItemAsync(Guid menuItemId, int page, int pageSize);
    Task<List<MyReviewDto>> GetMineAsync(Guid userId);
}