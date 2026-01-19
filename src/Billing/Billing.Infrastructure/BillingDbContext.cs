using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Billing.Infrastructure;

public sealed class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>().HasKey(i => i.Id);
        
        // Map InvoiceId value object to string in database
        modelBuilder.Entity<Invoice>()
            .Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => new Billing.Domain.ValueObjects.InvoiceId(value))
            .HasColumnType("nvarchar(450)");
        
        // OrderId is semantic string reference to Order
        modelBuilder.Entity<Invoice>()
            .Property(i => i.OrderId)
            .HasColumnType("nvarchar(450)");
        
        // Other configurations
        modelBuilder.Entity<Invoice>().Property(i => i.Status).IsRequired();
        modelBuilder.Entity<Invoice>().Property(i => i.Amount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Invoice>().Property(i => i.Currency).HasMaxLength(3);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
}
