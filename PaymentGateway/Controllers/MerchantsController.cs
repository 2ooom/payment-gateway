using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentGateway.Model;
using PaymentGateway.Services;

namespace PaymentGateway.Controllers
{
    /// <summary>
    /// End-points related to merchants (creation and authentication)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantsController : ControllerBase
    {
        private readonly IPaymentDbContext _paymentDb;
        private readonly IConfig _config;
        private readonly IEncryptionService _encryptionService;

        internal static readonly TimeSpan TokenValidity = TimeSpan.FromDays(7);

        public MerchantsController(
            IPaymentDbContext paymentDb,
            IConfig config,
            IEncryptionService encryptionService)
        {
            _paymentDb = paymentDb;
            _config = config;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Create/register new merchant. Access to Payment HTTP API is only possible after authentication
        /// </summary>
        /// <param name="request">Details about new merchant including login and password</param>
        /// <returns>In case of success new Merchant Id</returns>
        [HttpPost]
        public async Task<ActionResult<MerchantCreationResponse>> CreateMerchant([FromBody]MerchantCreationRequest request)
        {
            var existingMerchant = await _paymentDb.Merchants.FirstOrDefaultAsync(m => m.Login == request.Login);
            if (existingMerchant != null)
            {
                return BadRequest(new { message = "Merchant with this login already exists" });
            }
            var salt = _encryptionService.GenerateSalt();
            var merchant = new Merchant
            {
                Name = request.Name,
                Login = request.Login,
                Url = request.Url,
                AcquirerType = request.AcquirerType,
                Salt = salt,
                HashedPassword = _encryptionService.GetHash(request.Password, salt),
                Active = true,
            };
            _paymentDb.Merchants.Add(merchant);

            await _paymentDb.SaveChangesAsync();

            return new MerchantCreationResponse { Id = merchant.Id };
        }

        /// <summary>
        /// Authenticates merchant using login/password by providing JWT self-contained token
        /// </summary>
        /// <param name="request">Merchant's login and password</param>
        /// <returns>
        ///   1/ JWT Bearer token to be provided in Authorization HTTP header like so:
        ///        Authorization: Bearer {JwtToken}
        ///   2/ Expiry DateTime. After that point token will not be valid and client has to repeat
        ///      authentication process. By Default valid for 7 days.
        /// </returns>
        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthenticationResponse>> Authenticate([FromBody]AuthenticationRequest request)
        {
            var merchant = await _paymentDb.Merchants.SingleOrDefaultAsync(x => x.Login == request.Login);
            if (merchant == null || _encryptionService.GetHash(request.Password, merchant.Salt) != merchant.HashedPassword)
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.JwtSecret);
            var expires = DateTime.UtcNow.Add(TokenValidity);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, merchant.Id.ToString())
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthenticationResponse
            {
                Expires = expires,
                JwtToken = tokenHandler.WriteToken(token),
            };
        }
    }
}