using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentGateway.Client;
using PaymentGateway.Model;
using AcquirerType = PaymentGateway.Client.AcquirerType;
using MerchantCreationRequest = PaymentGateway.Client.MerchantCreationRequest;

namespace PaymentGateway.ITests
{
    [TestClass]
    public class MerchantTests
    {
        public PaymentGatewayFactory<Startup> Factory;
        public MerchantsClient MerchantClient;
        public HttpClient HttpClient;

        [TestInitialize]
        public void Setup()
        {
            Factory = new PaymentGatewayFactory<Startup>();
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
            var request = new MerchantCreationRequest
            {
                Name = "Amazon FR",
                AcquirerType = AcquirerType.Visa,
                Login = "amzn",
                Password = "123",
                Url = "http://amazon.fr"
            };
            var merchantCreate = await MerchantClient.CreateMerchantAsync(request);
            Assert.AreEqual(1L, merchantCreate.Id);

        }
    }
}
