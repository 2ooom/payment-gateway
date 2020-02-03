using PaymentGateway.Acquiring;

namespace PaymentGateway.Model
{
    public class PaymentResponse
    {
        public string Id { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string MaskedCardNumber { get; set; }
        public byte ExpiryMonth { get; set; }
        public ushort ExpiryYear { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
