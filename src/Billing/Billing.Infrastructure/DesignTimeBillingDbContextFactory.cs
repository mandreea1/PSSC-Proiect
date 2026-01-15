using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Design;

namespace Billing.Infrastructure;

public sealed class DesignTimeBillingDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BillingDbContext>();
        var cs = "Server=(localdb)\\MSSQLLocalDB;Database=BillingDb;Trusted_Connection=True;TrustServerCertificate=True";
        optionsBuilder
            .UseSqlServer(cs)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        return new BillingDbContext(optionsBuilder.Options);
        }
}
