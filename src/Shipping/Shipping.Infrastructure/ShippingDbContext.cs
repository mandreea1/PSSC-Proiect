using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shipping.Domain.Entities;
using Shipping.Domain.ValueObjects;

namespace Shipping.Infrastructure;

public sealed class ShippingDbContext : DbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var shipment = modelBuilder.Entity<Shipment>();
        shipment.HasKey(s => s.Id);
        
        // Map ShipmentId value object to string in database
        shipment.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => new ShipmentId(value))
            .HasColumnType("nvarchar(450)");
        
        // OrderId is semantic string reference to Order
        shipment.Property(s => s.OrderId)
            .HasColumnType("nvarchar(450)");
        
        shipment.Property(s => s.OrderId).IsRequired();
        shipment.Property(s => s.CustomerId).IsRequired();
        shipment.Property(s => s.Status).IsRequired();
        shipment.OwnsOne(s => s.Address, owned =>
        {
            owned.Property(a => a.Line1).HasColumnName("Line1").HasMaxLength(200);
            owned.Property(a => a.Line2).HasColumnName("Line2").HasMaxLength(200);
            owned.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
            owned.Property(a => a.Country).HasColumnName("Country").HasMaxLength(2);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
}
