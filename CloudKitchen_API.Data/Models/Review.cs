using System;
using System.Collections.Generic;

namespace CloudKitchen_API.Data.Models;

public partial class Review
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? OrderId { get; set; }

    public Guid MenuItemId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual MenuItem MenuItem { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual User User { get; set; } = null!;
}
