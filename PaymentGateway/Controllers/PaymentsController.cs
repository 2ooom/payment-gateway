﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Acquiring;
using PaymentGateway.Model;
using PaymentGateway.Services;

namespace PaymentGateway.Controllers
{
    /// <summary>
    /// End-point for payment submission/retrieval
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentDbContext _paymentDb;
        private readonly IBankRegistry _banksRegistry;
        private readonly IEncryptionService _encryptionService;

        public PaymentsController(
            IPaymentDbContext paymentDb,
            IBankRegistry banksRegistry,
            IEncryptionService encryptionService)
        {
            _paymentDb = paymentDb;
            _banksRegistry = banksRegistry;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Fetch all the all payment details including: acquiring bank id, amount, currency, payment status,
        /// masked card number and expiry.
        /// </summary>
        /// <returns>
        /// List of payment details including: amount, currency, payment status, masked card number and expiry
        /// Payment Status meaning:
        ///   Pending = 0
        ///   Accepted = 1
        ///   Refused = 2
        /// </returns>
        // GET: api/Payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            var merchantId = GetMerchantId();
            return await _paymentDb.Payments.Where(p => p.MerchantId == merchantId).ToListAsync();
        }

        /// <summary>
        /// Fetch specific payment submitted by current merchant (merchantId is taken from JWT token)
        /// </summary>
        /// <param name="id">Payment Id</param>
        /// <returns>
        /// Payment details including: acquiring bank id, amount, currency, payment status,
        /// masked card number and expiry.
        /// Payment Status meaning:
        ///   Pending = 0
        ///   Accepted = 1
        ///   Refused = 2
        /// </returns>
        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponse>> GetPayment(string id)
        {
            var payment = await _paymentDb.Payments.FindAsync(id);

            if (payment == null || payment.MerchantId != GetMerchantId())
            {
                return NotFound();
            }

            return GetPaymentResponse(payment);
        }

        /// <summary>
        /// Submit new payment
        /// </summary>
        /// <param name="request">Payment full details including card number and details (expiry, cvv, cardholder name)</param>
        /// <returns>
        /// Payment details including: acquiring bank id, amount, currency, payment status,
        /// masked card number and expiry.
        /// Payment Status meaning:
        ///   Pending = 0
        ///   Accepted = 1
        ///   Refused = 2
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<PaymentResponse>> PostPayment([FromBody]PaymentRequest request)
        {
            var merchantId = GetMerchantId();
            
            var merchant = await _paymentDb.Merchants.FindAsync(merchantId);
            if (merchant == null)
            {
                // TODO: log merchant absence
                return BadRequest();
            }

            if (!merchant.Active)
            {
                // TODO: log inactive merchant
                return BadRequest("This merchant is not active");
            }
            var bank = _banksRegistry.GetAcquirer(merchant);

            var response = await bank.SubmitPayment(request);
            var payment = new Payment
            {
                Id = response.AcquirerPaymentId,
                Amount = request.Amount,
                Currency = request.Currency,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                CardLastDigits = request.CardNumber.Substring(Math.Max(0, request.CardNumber.Length - 4)),
                CardNumberHashed = _encryptionService.GetHash(request.CardNumber, merchant.Salt),
                CardNumberLength = (byte)request.CardNumber.Length,
                MerchantId = merchantId,
                Status = response.Status,
                CreatedUtc = DateTime.UtcNow,
            };
            _paymentDb.Payments.Add(payment);
            await _paymentDb.SaveChangesAsync();

            return GetPaymentResponse(payment);
        }

        private static PaymentResponse GetPaymentResponse(Payment payment)
        {
            var maskedLen = Math.Max(0, payment.CardNumberLength - payment.CardLastDigits.Length);
            var mask = string.Join("", Enumerable.Range(0, maskedLen).Select(x => "X"));
            return new PaymentResponse
            {
                Id = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                MaskedCardNumber = mask + payment.CardLastDigits,
                Status = payment.Status,
            };
        }

        private long GetMerchantId()
        {
            var merchantId = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
            return long.Parse(merchantId);
        }
    }
}
