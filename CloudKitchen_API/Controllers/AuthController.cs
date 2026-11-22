using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CloudKitchen_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var result = await _authService.CompleteProfileAsync(request.Mobile, request.FullName, request.Email);
                var token = CreateToken(result);
                result.Token = token;
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "User register successfully",
                    Data = result
                });
            }
            catch (ApiException ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var result = await _authService.RequestOtpAsync(request.Mobile);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result,
                    Message = "OTP sent successfully."
                });
            }catch(ApiException ex)
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var result = await _authService.VerifyOtpAsync(request.Mobile, request.OtpCode);
                if (!result.IsNewUser)
                {
                    var token = CreateToken(result);
                    result.Token = token;
                }
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result
                });

            }
            catch (ApiException ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        private string CreateToken(UserDto user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, user.Mobile),
            };

            // Get Secret Key from appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Token valid for 7 days
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}