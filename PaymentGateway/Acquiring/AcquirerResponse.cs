using PaymentGateway.Model;

namespace PaymentGateway.Acquiring
{
    public class AcquirerResponse
    {
        public string AcquirerPaymentId { get; set; }
        public PaymentStatus Status { get; set; }
        public long MerchantId { get; set; }
    }
}
