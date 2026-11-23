using Microsoft.EntityFrameworkCore;

namespace CloudKitchen_API.Data.Models
{
    public partial class KitchenDbContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                        .Property(o => o.Version)
                        .IsRowVersion();
        }
    }
}
