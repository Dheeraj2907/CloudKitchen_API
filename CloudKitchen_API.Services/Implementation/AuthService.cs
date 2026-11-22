using CloudKitchen_API.Data.Models;
using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Implementation
{
    public class AuthService : IAuthService
    {

        private readonly KitchenDbContext _dbContext;

        public AuthService(KitchenDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1. REQUEST OTP (Handles both Login and Auto-Register)
        public async Task<object> RequestOtpAsync(string mobile)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Mobile == mobile);
            bool isNewUser = false;

            if (user == null)
            {
                // User doesn't exist. Create a skeleton user.
                isNewUser = true;
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Mobile = mobile,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Users.Add(user);
            }

            // Generate OTP and update expiry
            var otp = GetLastSixDigits(mobile);
            //user.OtpCode = otp;
            //user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);

            await _dbContext.SaveChangesAsync();

            return new
            {
                IsNewUser = isNewUser,
            };
        }

        // 2. VERIFY OTP
        public async Task<UserDto> VerifyOtpAsync(string mobile, string otpCode)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Mobile == mobile);
            if (user == null)
            {
                throw new ApiException("User not found");
            }

            //if (user.OtpCode != otpCode) throw new ApiException("Invalid OTP" );
            //if (user.OtpExpiryTime < DateTime.UtcNow) throw new ApiException("OTP has expired");

            //// Clear OTP
            //user.OtpCode = null;
            //user.OtpExpiryTime = null;
            await _dbContext.SaveChangesAsync();

            // Check if user is "new" (i.e., they haven't set their name yet)
            bool isNewUser = string.IsNullOrEmpty(user.FullName);

            if (isNewUser)
            {
                // Ask the app to show the Complete Profile screen
                return new UserDto
                {
                    IsNewUser = true,
                    Mobile = user.Mobile
                };
            }
            else
            {
                // Existing user! Generate JWT immediately.
                return new UserDto
                {
                    IsNewUser = false,
                    Id = user.Id,
                    FullName = user.FullName,
                    Mobile = user.Mobile,
                    Email = user.Email
                };
            }
        }

        public async Task<UserDto> CompleteProfileAsync(string mobile, string fullName, string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Mobile == mobile);
            if (user == null) throw new ApiException("User not found");

            // Update details
            user.FullName = fullName;
            user.Email = email;
            await _dbContext.SaveChangesAsync();


            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Mobile = user.Mobile,
                Email = user.Email
            };

        }

        // Helper method to generate a 6-digit OTP
        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public string GetLastSixDigits(string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
                return string.Empty;

            // 1. Filter out everything that is NOT a digit (0-9)
            var digitsOnly = new string(mobile.Where(char.IsDigit).ToArray());

            // 2. Get the last 6 digits (or fewer if the string is shorter than 6)
            if (digitsOnly.Length > 6)
            {
                return digitsOnly.Substring(digitsOnly.Length - 6);
            }

            return digitsOnly;
        }
    }
}
