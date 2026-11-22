using CloudKitchen_API.Services.Dto;

namespace CloudKitchen_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<object> RequestOtpAsync(string mobile);
        Task<UserDto> VerifyOtpAsync(string mobile, string otpCode);
        Task<UserDto> CompleteProfileAsync(string mobile, string fullName, string email);
    }
}
