using System.Threading.Tasks;
using PaymentGateway.Model;

namespace PaymentGateway.Acquiring
{
    public interface IAcquirer
    {
        Task<AcquirerResponse> SubmitPayment(PaymentRequest request);
    }
}
