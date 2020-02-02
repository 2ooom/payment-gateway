using System.Threading.Tasks;
using PaymentGateway.Model;

namespace PaymentGateway.Services
{
    public interface IAuthenticationService
    {
        Task<MerchantCreationResponse> CreateMerchant(MerchantCreationRequest request);
        Task<AuthenticationResponse> Authenticate(AuthenticationRequest request);
    }
}