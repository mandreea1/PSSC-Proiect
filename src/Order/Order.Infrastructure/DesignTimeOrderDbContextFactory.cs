using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Order.Infrastructure;

public sealed class DesignTimeOrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        var cs = "Server=(localdb)\\MSSQLLocalDB;Database=OrderDb;Trusted_Connection=True;TrustServerCertificate=True";
        optionsBuilder.UseSqlServer(cs);
        return new OrderDbContext(optionsBuilder.Options);
    }
}
