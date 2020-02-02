using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Acquiring;
using PaymentGateway.Model;

namespace PaymentGateway.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentDbContext _paymentDb;
        private readonly IBankRegistry _banksRegistry;

        public PaymentsController(IPaymentDbContext paymentDb, IBankRegistry banksRegistry)
        {
            _paymentDb = paymentDb;
            _banksRegistry = banksRegistry;
        }

        // GET: api/Payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPayments()
        {
            var merchantId = GetMerchantId();
            return await _paymentDb.Payments.Where(p => p.MerchantId == merchantId).ToListAsync();
        }

        // GET: api/Payments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(long id)
        {
            var payment = await _paymentDb.Payments.FindAsync(id);

            if (payment == null || payment.MerchantId != GetMerchantId())
            {
                return NotFound();
            }

            return payment;
        }

        [HttpPost]
        public async Task<ActionResult<Payment>> PostPayment(PaymentRequest request)
        {
            var merchantId = GetMerchantId();
            var payment = new Payment
            {
                Amount = request.Amount,
                Currency = request.Currency,
                CardLastDigits = request.CardNumber.Substring(Math.Max(0, request.CardNumber.Length - 4)),
                MerchantId = merchantId,
                Status = PaymentStatus.Pending,
                CreatedUtc = DateTime.UtcNow,
            };
            _paymentDb.Payments.Add(payment);
            var createTask = _paymentDb.SaveChangesAsync();
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

            await createTask.ContinueWith(t =>
            {
                payment.UpdatedUtc = DateTime.UtcNow;
                payment.Status = response.Status;
                payment.AcquirerId = response.AcquirerPaymentId;
                return _paymentDb.SaveChangesAsync();
            });

            return payment;
        }

        private long GetMerchantId()
        {
            var merchantId = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
            return long.Parse(merchantId);
        }
    }
}
