using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Design;

namespace Shipping.Infrastructure;

public sealed class DesignTimeShippingDbContextFactory : IDesignTimeDbContextFactory<ShippingDbContext>
{
    public ShippingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ShippingDbContext>();
        var cs = "Server=(localdb)\\MSSQLLocalDB;Database=ShippingDb;Trusted_Connection=True;TrustServerCertificate=True";
        optionsBuilder
            .UseSqlServer(cs)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        return new ShippingDbContext(optionsBuilder.Options);
    }
}
