using System.ComponentModel.DataAnnotations;

namespace CloudKitchen_API.Model
{
    public class RegisterRequest
    {
        [Required]
        public string? FullName { get; set; }

        [Required]
        [Phone]
        public string? Mobile { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
    public class RegisterResponse : RegisterRequest
    {
        public Guid Id { get; set; }
    }
}
