using System;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PaymentGateway.Controllers;
using PaymentGateway.Model;

namespace PaymentGateway.Tests.UnitTests
{
    [TestClass]
    public class MerchantControllerTests
    {
        public MerchantsController Subject;
        public PaymentDbContext PaymentDb;
        public Mock<IConfig> Config;

        [TestInitialize]
        public void Setup()
        {
            Config = new Mock<IConfig>();
            var dbOptions = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(databaseName: "PaymentDb")
                .Options;

            Config.Setup(t => t.JwtSecret)
                // SHA256
                .Returns("7FDA29706E24A6E44DB4669CF85EE6CE88C65342845AE08EBC9FEF621B73110E");
            PaymentDb = new PaymentDbContext(dbOptions);
            Subject = new MerchantsController(PaymentDb, Config.Object);
        }

        [TestMethod]
        public async Task TestCreateMerchantSuccess()
        {
            var request = GetMerchantRequest();
            var response = await Subject.CreateMerchant(request);
            Console.WriteLine(response);
            var merchantId = response.Value.Id;
            var merchant = PaymentDb.Merchants.Single(m => m.Id == merchantId);
            Assert.AreEqual(request.Name, merchant.Name);
            Assert.AreEqual(request.Login, merchant.Login);
            Assert.AreEqual(request.Url, merchant.Url);
            Assert.AreEqual(request.AcquirerType, merchant.AcquirerType);
            Assert.IsTrue(merchant.Active);
            Assert.IsNotNull(merchant.Salt);
            var hashedPassword = MerchantsController.GetHashedPassword(request.Password, merchant.Salt);
            Assert.AreEqual(hashedPassword, merchant.HashedPassword);
        }

        [TestMethod]
        public async Task TestCreateMerchantAlreadyExists()
        {
            var request = GetMerchantRequest();
            var response = await Subject.CreateMerchant(request);
            var merchantId = response.Value.Id;
            Assert.IsTrue(PaymentDb.Merchants.Any(m => m.Id == merchantId));
            response = await Subject.CreateMerchant(new MerchantCreationRequest
            {
                Login = request.Login,
                Name = "New merchant",
                Password = "OtherPassword"
            });
            Assert.IsInstanceOfType(response.Result, typeof(BadRequestObjectResult));
            Assert.IsNull(response.Value);
        }

        [TestMethod]
        public async Task TestAuthenticateMerchantNoUser()
        {
            var tokenResponse = await Subject.Authenticate(new AuthenticationRequest
            {
                Login = "login",
                Password = "password",
            });
            Assert.IsInstanceOfType(tokenResponse.Result, typeof(BadRequestObjectResult));
            Assert.IsNull(tokenResponse.Value);
        }

        [TestMethod]
        public async Task TestAuthenticateMerchantPasswordMismatch()
        {
            var request = GetMerchantRequest();
            await Subject.CreateMerchant(request);
            var tokenResponse = await Subject.Authenticate(new AuthenticationRequest
            {
                Login = request.Login,
                Password = "wrong-password",
            });
            Assert.IsInstanceOfType(tokenResponse.Result, typeof(BadRequestObjectResult));
            Assert.IsNull(tokenResponse.Value);
        }

        [TestMethod]
        public async Task TestAuthenticateMerchantSuccess()
        {
            var now = DateTime.UtcNow;
            var request = GetMerchantRequest();
            await Subject.CreateMerchant(request);
            var tokenResponse = await Subject.Authenticate(new AuthenticationRequest
            {
                Login = request.Login,
                Password = request.Password,
            });
            var token = tokenResponse.Value;
            Assert.IsNotNull(token.JwtToken);
            Assert.IsTrue(now.Add(MerchantsController.TokenValidity) <= token.Expires);
        }

        [TestCleanup]
        public void Cleanup()
        {
            PaymentDb.Database.EnsureDeleted();
            PaymentDb?.Dispose();
        }

        private static MerchantCreationRequest GetMerchantRequest()
        {
            return new MerchantCreationRequest
            {
                Name = "Amazon Inc",
                Login = "amazon",
                Password = "123",
                AcquirerType = AcquirerType.MasterCard,
            };
        }
    }
}
