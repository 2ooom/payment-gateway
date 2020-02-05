using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentGateway.Client;
using PaymentGateway.Model;
using PaymentGateway.Tests;

namespace PaymentGateway.ITests
{
    [TestClass]
    public class MerchantTests
    {
        public PaymentGatewayServiceFactory<Startup> Factory;
        public MerchantsClient MerchantClient;
        public HttpClient HttpClient;

        [TestInitialize]
        public void Setup()
        {
            Factory = new PaymentGatewayServiceFactory<Startup>();
            HttpClient = Factory.CreateClient();
            MerchantClient = new MerchantsClient(HttpClient.BaseAddress.ToString(), HttpClient);
        }

        [TestCleanup]
        public void Cleanup()
        {
            HttpClient?.Dispose();
            Factory?.Dispose();
        }

        [TestMethod]
        public async Task CreateMerchant()
        {
            var request = MerchantControllerTests.GetMerchantRequest();
            var merchantCreate = await MerchantClient.CreateMerchantAsync(request);
            Assert.AreEqual(1L, merchantCreate.Id);
        }

        [TestMethod]
        public async Task AuthenticateMerchant()
        {
            var expectedExpiryDate = DateTime.UtcNow.AddDays(7);
            var request = MerchantControllerTests.GetMerchantRequest();
            await MerchantClient.CreateMerchantAsync(request);
            var jwtResponse = await MerchantClient.AuthenticateAsync(new AuthenticationRequest
            {
                Login = request.Login,
                Password = request.Password,
            });
            Assert.IsTrue(jwtResponse.JwtToken.Length > 0);
            Assert.IsTrue(jwtResponse.Expires > expectedExpiryDate);
        }
    }
}
