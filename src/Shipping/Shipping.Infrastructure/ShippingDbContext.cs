using Microsoft.EntityFrameworkCore;
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
        shipment.Property(s => s.OrderId).IsRequired();
        shipment.Property(s => s.CustomerId).IsRequired();
        shipment.Property(s => s.Status).HasConversion<int>();
        shipment.OwnsOne(s => s.Address, owned =>
        {
            owned.Property(a => a.Line1).HasColumnName("Line1").HasMaxLength(200);
            owned.Property(a => a.Line2).HasColumnName("Line2").HasMaxLength(200);
            owned.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
            owned.Property(a => a.Country).HasColumnName("Country").HasMaxLength(2);
        });
    }
}
