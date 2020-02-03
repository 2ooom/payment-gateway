using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentGateway.Model;

namespace PaymentGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantsController : ControllerBase
    {
        private readonly IPaymentDbContext _paymentDb;
        private readonly IConfig _config;

        private const int NbHashIterations = 1000;
        internal static readonly TimeSpan TokenValidity = TimeSpan.FromDays(7);

        public MerchantsController(IPaymentDbContext paymentDb, IConfig config)
        {
            _paymentDb = paymentDb;
            _config = config;
        }

        [HttpPost]
        public async Task<ActionResult<MerchantCreationResponse>> CreateMerchant([FromBody]MerchantCreationRequest request)
        {
            var existingMerchant = await _paymentDb.Merchants.FirstOrDefaultAsync(m => m.Login == request.Login);
            if (existingMerchant != null)
            {
                return BadRequest(new { message = "Merchant with this login already exists" });
            }
            var salt = GenerateSalt();
            var merchant = new Merchant
            {
                Name = request.Name,
                Login = request.Login,
                Url = request.Url,
                AcquirerType = request.AcquirerType,
                Salt = salt,
                HashedPassword = GetHashedPassword(request.Password, salt),
                Active = true,
            };
            _paymentDb.Merchants.Add(merchant);

            await _paymentDb.SaveChangesAsync();

            return new MerchantCreationResponse { Id = merchant.Id };
        }

        private static string GenerateSalt()
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return Convert.ToBase64String(salt);
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthenticationResponse>> Authenticate([FromBody]AuthenticationRequest request)
        {
            var merchant = await _paymentDb.Merchants.SingleOrDefaultAsync(x => x.Login == request.Login);
            if (merchant == null || GetHashedPassword(request.Password, merchant.Salt) != merchant.HashedPassword)
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

        internal static string GetHashedPassword(string password, string salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: NbHashIterations,
                numBytesRequested: 256 / 8));
        }
    }
}