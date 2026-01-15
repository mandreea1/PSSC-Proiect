using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var o = modelBuilder.Entity<OrderEntity>();
        o.ToTable("Orders");
        o.HasKey(x => x.Id);
        o.Property(x => x.CustomerId).IsRequired();
        o.Property(x => x.Total).HasColumnType("decimal(18,2)");
        o.Property(x => x.Status).HasConversion<int>();
        o.Property(x => x.CreatedAt).HasColumnType("datetime2");
    }
}
