using System;
using System.Collections.Generic;

namespace CloudKitchen_API.Data.Models;

public partial class CartItem
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual MenuItem MenuItem { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
