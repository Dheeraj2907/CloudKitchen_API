using System;
using System.Collections.Generic;
using System.Text;

namespace CloudKitchen_API.Services.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;

        public string Mobile { get; set; } = null!;

        public string Email { get; set; } = null!;

        public bool IsNewUser { get; set; }
        public string? Token { get; set; }
    }
}
