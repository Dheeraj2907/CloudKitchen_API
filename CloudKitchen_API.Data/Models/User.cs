using System;
using System.Collections.Generic;

namespace CloudKitchen_API.Data.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string? FullName { get; set; }

    public string Mobile { get; set; } = null!;

    public string? Email { get; set; }

    public string? OtpCode { get; set; }

    public DateTime? OtpExpiryTime { get; set; }

    public DateTime? CreatedAt { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
