using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Model;

namespace PaymentGateway.ITests
{
    public class PaymentGatewayFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // To replace original PaymentDbContext registration with in-memory stub
                var dbContextServicesToOverride = new[]
                {
                    typeof(DbContextOptions<PaymentDbContext>),
                    typeof(IPaymentDbContext),
                    typeof(PaymentDbContext)
                };
                var descriptorsToRemove = services.Where(
                    d => dbContextServicesToOverride.Contains(d.ServiceType)).ToArray();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using an in-memory database for testing.
                services.AddDbContext<IPaymentDbContext, PaymentDbContext>(options =>
                {
                    options.UseInMemoryDatabase("PaymentDbInMemory");
                });
                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context singleton (PaymentDbContext).
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<IPaymentDbContext>();
                    // Recreate
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    // Ensure the database is created.
                    db.Database.EnsureCreated();
                }
            });
        }
    }
}
