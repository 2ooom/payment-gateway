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
        public async Task<ActionResult<Payment>> PostPayment(PaymentRequest paymentRequest)
        {
            // TODO: Verify currency

            var payment = new Payment
            {
                Amount = paymentRequest.Amount,
                Currency = paymentRequest.Currency
            };
            _paymentDb.Payments.Add(payment);
            await _paymentDb.SaveChangesAsync();

            return CreatedAtAction("GetPayment", new { id = payment.Id }, payment);
        }

        private long GetMerchantId()
        {
            var merchantId = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
            return long.Parse(merchantId);
        }
    }
}
