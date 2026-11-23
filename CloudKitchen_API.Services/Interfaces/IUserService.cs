using CloudKitchen_API.Services.Dto;
namespace CloudKitchen_API.Services.Interfaces;

public interface IUserService
{
    Task<ProfileResponse> GetProfileAsync(Guid userId);
    Task<ProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}