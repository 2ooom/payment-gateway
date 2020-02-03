using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PaymentGateway.Acquiring;
using PaymentGateway.Controllers;
using PaymentGateway.Model;

namespace PaymentGateway.Tests.UnitTests
{
    [TestClass]
    public class PaymentsControllerTests : BaseControllerTests
    {
        public PaymentsController Subject;
        public Merchant Merchant;

        public Mock<IBankRegistry> BankRegistry;
        public Mock<IAcquirer> Acquirer;

        public const long UnknownMerchantId = 999;

        [TestInitialize]
        public override async Task Setup()
        {
            await base.Setup();
            BankRegistry = new Mock<IBankRegistry>();
            Merchant = new Merchant
            {
                AcquirerType = AcquirerType.MasterCard,
                Active = true,
                HashedPassword = "123",
                Id = 1,
                Login = "merchant",
                Salt = EncryptionService.GenerateSalt(),
                Name = "Some Merchant"
            };
            PaymentDb.Merchants.Add(Merchant);
            await PaymentDb.SaveChangesAsync();
            Acquirer = new Mock<IAcquirer>();
            BankRegistry.Setup(t => t.GetAcquirer(Merchant))
                .Returns(Acquirer.Object);
            Subject = new PaymentsController(PaymentDb, BankRegistry.Object, EncryptionService);
            SetMerchantId(Subject, Merchant.Id);
        }

        [TestMethod]
        public async Task TestCreatePaymentUnknownMerchant()
        {
            SetMerchantId(Subject, UnknownMerchantId);
            var paymentRequest = GetPaymentRequest();
            var response = await Subject.PostPayment(paymentRequest);
            Assert.IsInstanceOfType(response.Result, typeof(BadRequestResult));
            Assert.IsNull(response.Value);
        }

        [TestMethod]
        public async Task TestCreatePaymentInactiveMerchant()
        {
            Merchant.Active = false;
            await PaymentDb.SaveChangesAsync();
            var paymentRequest = GetPaymentRequest();
            var response = await Subject.PostPayment(paymentRequest);
            Assert.IsInstanceOfType(response.Result, typeof(BadRequestObjectResult));
            Assert.IsNull(response.Value);
        }

        [TestMethod]
        public async Task TestCreatePaymentSuccess()
        {
            var paymentRequest = GetPaymentRequest();

            var acquirerResponse = new AcquirerResponse
            {
                AcquirerPaymentId = "0123-454543-aa",
                MerchantId = Merchant.Id,
                Status = PaymentStatus.Accepted
            };
            Acquirer.Setup(t => t.SubmitPayment(paymentRequest))
                .Returns(Task.FromResult(acquirerResponse));
            var response = await Subject.PostPayment(paymentRequest);
            var paymentResponse = response.Value;
            Assert.AreEqual(acquirerResponse.AcquirerPaymentId, paymentResponse.Id);
            Assert.AreEqual(acquirerResponse.Status, paymentResponse.Status);
        }

        [TestMethod]
        public async Task TestCreatePaymentRefused()
        {
            var paymentRequest = GetPaymentRequest();

            var acquirerResponse = new AcquirerResponse
            {
                AcquirerPaymentId = "0123-454543-bb",
                MerchantId = Merchant.Id,
                Status = PaymentStatus.Refused
            };
            Acquirer.Setup(t => t.SubmitPayment(paymentRequest))
                .Returns(Task.FromResult(acquirerResponse));
            var response = await Subject.PostPayment(paymentRequest);
            var paymentResponse = response.Value;
            Assert.AreEqual(acquirerResponse.AcquirerPaymentId, paymentResponse.Id);
            Assert.AreEqual(acquirerResponse.Status, paymentResponse.Status);
        }

        [TestMethod]
        public async Task TestGetPaymentNotCurrentMerchant()
        {
            var payment = GetPayment(UnknownMerchantId);
            PaymentDb.Payments.Add(payment);
            await PaymentDb.SaveChangesAsync();
            var response = await Subject.GetPayment(payment.Id);
            Assert.IsInstanceOfType(response.Result, typeof(NotFoundResult));
            Assert.IsNull(response.Value);
        }

        [TestMethod]
        public async Task TestGetPaymentSuccess()
        {
            var payment = GetPayment(Merchant.Id);
            PaymentDb.Payments.Add(payment);
            await PaymentDb.SaveChangesAsync();
            var response = await Subject.GetPayment(payment.Id);
            var paymentResponse = response.Value;
            Assert.AreEqual(payment.Id, paymentResponse.Id);
            Assert.AreEqual(payment.Status, paymentResponse.Status);
            Assert.AreEqual(payment.Amount, paymentResponse.Amount);
            Assert.AreEqual(payment.Currency, paymentResponse.Currency);
            Assert.AreEqual(payment.ExpiryMonth, paymentResponse.ExpiryMonth);
            Assert.AreEqual(payment.ExpiryYear, paymentResponse.ExpiryYear);
            Assert.AreEqual("XXXXXX" + payment.CardLastDigits, paymentResponse.MaskedCardNumber);
        }

        [TestMethod]
        public async Task TestGetPaymentsEmpty()
        {
            var payments = new[]
            {
                GetPayment(UnknownMerchantId),
            };
            PaymentDb.Payments.AddRange(payments);
            await PaymentDb.SaveChangesAsync();
            var response = await Subject.GetPayments();
            var paymentsResponse = response.Value.ToArray();
            Assert.AreEqual(0, paymentsResponse.Length);
        }

        [TestMethod]
        public async Task TestGetPaymentsForSameMerchant()
        {
            var payments = new[]
            {
                GetPayment(Merchant.Id),
                GetPayment(UnknownMerchantId),
                GetPayment(Merchant.Id),
            };
            PaymentDb.Payments.AddRange(payments);
            await PaymentDb.SaveChangesAsync();
            var response = await Subject.GetPayments();
            var paymentsResponse = response.Value.ToArray();
            Assert.AreEqual(2, paymentsResponse.Length);
        }

        private static Payment GetPayment(long merchantId)
        {
            return new Payment
            {
                Id = Guid.NewGuid().ToString(),
                Amount = 15.7d,
                Currency = "EUR",
                CardNumberHashed = "=ABC23423FE342=",
                CardNumberLength = 10,
                CardLastDigits = "7890",
                ExpiryMonth = 07,
                ExpiryYear = 2021,
                Status = PaymentStatus.Accepted,
                MerchantId = merchantId,
            };
        }

        private static void SetMerchantId(ControllerBase controller, long merchantId)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new []
                    {
                        new Claim(ClaimTypes.Name, merchantId.ToString())
                    })),
                }
            };
        }

        private static PaymentRequest GetPaymentRequest()
        {
            return new PaymentRequest
            {
                Amount = 15.7d,
                Currency = "EUR",
                CardHolderName = "MR JOHN SMITH",
                CardNumber = "1234567890",
                Cvv = "0782",
                ExpiryMonth = 07,
                ExpiryYear = 2021,
            };
        }
    }
}
