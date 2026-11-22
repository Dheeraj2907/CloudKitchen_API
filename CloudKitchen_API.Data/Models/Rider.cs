using System;
using System.Collections.Generic;

namespace CloudKitchen_API.Data.Models;

public partial class Rider
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Mobile { get; set; } = null!;

    public string? VehicleNumber { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
