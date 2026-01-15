using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure;

public sealed class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>().HasKey(i => i.Id);
        // rely on conventions for required properties
        modelBuilder.Entity<Invoice>().Property(i => i.Status).HasConversion<int>();
        modelBuilder.Entity<Invoice>().Property(i => i.Amount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Invoice>().Property(i => i.Currency).HasMaxLength(3);
    }
}
