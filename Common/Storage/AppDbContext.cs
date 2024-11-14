using Common.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Storage;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DiscountCode> DiscountCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscountCode>()
            .HasIndex(u => u.Code)
            .IsUnique();
    }
}
