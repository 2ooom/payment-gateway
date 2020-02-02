namespace PaymentGateway.Acquiring
{
    public class AcquirerResponse
    {
        public string Id { get; set; }
        public PaymentStatus Status { get; set; }
        public long MerchantId { get; set; }
    }
}
