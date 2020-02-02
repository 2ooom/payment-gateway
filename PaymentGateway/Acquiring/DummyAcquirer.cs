using System;
using System.Threading.Tasks;
using PaymentGateway.Model;

namespace PaymentGateway.Acquiring
{
    public class DummyAcquirer : IAcquirer
    {
        private readonly Merchant _merchant;

        public DummyAcquirer(Merchant merchant)
        {
            _merchant = merchant;
        }

        public Task<AcquirerResponse> SubmitPayment(PaymentRequest request)
        {
            return Task.Delay(TimeSpan.FromMilliseconds(1500)).ContinueWith(t => new AcquirerResponse
            {
                AcquirerPaymentId = Guid.NewGuid().ToString(),
                MerchantId = _merchant.Id,
                Status = PaymentStatus.Accepted,
            });
        }
    }
}
