using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Acquiring;
using PaymentGateway.Model;

namespace PaymentGateway.ITests
{
    public class PaymentGatewayServiceFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // We replace several original with in-memory stubs
                var servicesToOverride = new[]
                {
                    // PaymentDbContext related
                    typeof(DbContextOptions<PaymentDbContext>),
                    typeof(IPaymentDbContext),
                    typeof(PaymentDbContext),
                    // Acquiring banks
                    typeof(IBankRegistry),
                };
                RemoveFromServiceRegistry(services, servicesToOverride);

                // Add PaymentDbContext using an in-memory database for testing.
                services.AddDbContext<IPaymentDbContext, PaymentDbContext>(options =>
                {
                    options.UseInMemoryDatabase("PaymentDbInMemory");
                });
                services.AddSingleton<IBankRegistry>(new BankRegistryMock());
                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context singleton (PaymentDbContext).
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<IPaymentDbContext>();
                // Delete and recreate database from scratch
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            });
        }

        private static void RemoveFromServiceRegistry(IServiceCollection services, Type[] servicesToOverride)
        {
            var descriptorsToRemove = services.Where(
                d => servicesToOverride.Contains(d.ServiceType)).ToArray();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
        }
    }
}
