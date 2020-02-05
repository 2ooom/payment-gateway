using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PaymentGateway.Acquiring;
using PaymentGateway.Client;
using PaymentGateway.Model;
using PaymentGateway.Tests;

namespace PaymentGateway.ITests
{
    [TestClass]
    public class PaymentTests
    {
        public PaymentGatewayServiceFactory<Startup> Factory;
        public PaymentsClient PaymentClient;
        public MerchantsClient MerchantClient;
        public HttpClient HttpClient;
        public MerchantCreationRequest Merchant;
        public long MerchantId;

        [TestInitialize]
        public async Task Setup()
        {
            Factory = new PaymentGatewayServiceFactory<Startup>();
            HttpClient = Factory.CreateClient();
            // Create merchant
            Merchant = MerchantControllerTests.GetMerchantRequest();
            MerchantClient = new MerchantsClient(HttpClient.BaseAddress.ToString(), HttpClient);

            // Authenticate Merchant
            var merchantResponse = await MerchantClient.CreateMerchantAsync(Merchant);
            MerchantId = merchantResponse.Id;
        }

        [TestCleanup]
        public void Cleanup()
        {
            HttpClient?.Dispose();
            Factory?.Dispose();
        }

        [TestMethod]
        public async Task SubmitPayment()
        {
            var paymentRequest = PaymentsControllerTests.GetPaymentRequest();
            var acquirer = new Mock<IAcquirer>();
            BankRegistryMock.SetAcquirer(MerchantId, acquirer.Object);

            var acquirerResponse = new AcquirerResponse
            {
                AcquirerPaymentId = "0123-454543-bb",
                MerchantId = MerchantId,
                Status = PaymentStatus.Refused
            };
            acquirer.Setup(t => t.SubmitPayment(It.IsAny<PaymentRequest>()))
                .Returns(Task.FromResult(acquirerResponse));

            // Get JWT
            var jwt = await MerchantClient.AuthenticateAsync(new AuthenticationRequest
            {
                Login = Merchant.Login,
                Password = Merchant.Password,
            });

            PaymentClient = new PaymentsClient(HttpClient.BaseAddress.ToString(), HttpClient, () => jwt.JwtToken);
            var paymentResponse = await PaymentClient.PostPaymentAsync(paymentRequest);
            Assert.AreEqual(acquirerResponse.AcquirerPaymentId, paymentResponse.Id);
            Assert.AreEqual(acquirerResponse.Status, paymentResponse.Status);
            Assert.AreEqual(paymentRequest.Amount, paymentResponse.Amount);
            Assert.AreEqual(paymentRequest.Currency, paymentResponse.Currency);
            Assert.AreEqual(paymentRequest.ExpiryMonth, paymentResponse.ExpiryMonth);
            Assert.AreEqual(paymentRequest.ExpiryYear, paymentResponse.ExpiryYear);
            Assert.AreEqual("XXXXXX7890", paymentResponse.MaskedCardNumber);
        }

        [TestMethod]
        public async Task SubmitPaymentUnauthorized()
        {
            var paymentRequest = PaymentsControllerTests.GetPaymentRequest();
            PaymentClient = new PaymentsClient(HttpClient.BaseAddress.ToString(), HttpClient, () => "wrong-token");
            try
            {
                await PaymentClient.PostPaymentAsync(paymentRequest);
                Assert.Fail();
            }
            catch (ApiException e)
            {
                Assert.AreEqual(401, e.StatusCode);
            }
        }
    }
}
