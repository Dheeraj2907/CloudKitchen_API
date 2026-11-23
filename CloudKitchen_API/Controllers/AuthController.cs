using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Extensions;
using CloudKitchen_API.Model;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest req, [FromServices] KitchenDbContext db)
        {
            var userId = User.GetUserId();

            if (!string.IsNullOrWhiteSpace(req.RefreshToken))
            {
                var rt = await db.RefreshTokens
                    .FirstOrDefaultAsync(r => r.Token == req.RefreshToken && r.UserId == userId);
                if (rt != null && rt.RevokedAt == null)
                {
                    rt.RevokedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            }

            // Optional: unregister this device from push (client sends token if it wants)
            // db.DeviceTokens.RemoveRange(db.DeviceTokens.Where(d => d.UserId == userId && d.Token == ...));
            // await db.SaveChangesAsync();

            return Ok(new ApiResponse { Success = true, Message = "Logged out." });
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

