using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentGateway.Model;
using PaymentGateway.Services;

namespace PaymentGateway.Tests
{
    public class BaseControllerTests
    {
        public PaymentDbContext PaymentDb;
        public IEncryptionService EncryptionService;

        [TestInitialize]
        public virtual Task Setup()
        {
            var dbOptions = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(databaseName: "PaymentDb")
                .Options;
            EncryptionService = new EncryptionService();
            PaymentDb = new PaymentDbContext(dbOptions);
            return Task.CompletedTask;
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            PaymentDb.Database.EnsureDeleted();
            PaymentDb?.Dispose();
        }
    }
}
