using System.ComponentModel.DataAnnotations;

namespace CloudKitchen_API.Model
{
    public class LoginRequest
    {
        [Required]
        [Phone]
        public string? Mobile { get; set; }
    }
}
