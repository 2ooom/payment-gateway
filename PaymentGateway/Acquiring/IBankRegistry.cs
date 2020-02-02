using PaymentGateway.Model;

namespace PaymentGateway.Acquiring
{
    public interface IBankRegistry
    {
        IAcquirer GetAcquirer(Merchant merchant);
    }
}