using CloudKitchen_API.Services.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Mobile { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public Roles Role { get; set; } = Roles.Customer;   // ◀ add
        public bool IsNewUser { get; set; }
        public string? Token { get; set; }
    }
}
