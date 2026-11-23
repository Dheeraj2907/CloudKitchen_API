using CloudKitchen_API.Services.Dto;
namespace CloudKitchen_API.Services.Interfaces;

public interface ICouponService
{
    Task<List<CouponListDto>> GetActiveAsync();
    Task<ValidateCouponResponse> ValidateAsync(ValidateCouponRequest request);
}