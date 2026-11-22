using System.ComponentModel.DataAnnotations;

namespace CloudKitchen_API.Model
{
    public class VerifyOtpRequest
    {
        [Required]
        [Phone]
        public string? Mobile { get; set; }

        [Required]
        [MinLength(6), MaxLength(6)]
        public string? OtpCode { get; set; }
    }
}
