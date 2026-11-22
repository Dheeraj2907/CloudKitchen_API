using System;
using System.Collections.Generic;

namespace CloudKitchen_API.Data.Models;

public partial class Orderitem
{
    public int Id { get; set; }

    public Guid? Orderid { get; set; }

    public int? Menuitemid { get; set; }

    public int Quantity { get; set; }

    public decimal Unitprice { get; set; }

    public virtual MenuItem? Menuitem { get; set; }

    public virtual Order? Order { get; set; }
}
