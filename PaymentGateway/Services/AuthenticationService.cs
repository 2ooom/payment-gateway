using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentGateway.Model;

namespace PaymentGateway.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IPaymentDbContext _paymentDb;
        private readonly IConfig _config;

        private const int NbHashIterations = 1000;
        public AuthenticationService(IPaymentDbContext paymentDb, IConfig config)
        {
            _paymentDb = paymentDb;
            _config = config;
        }

        public async Task<MerchantCreationResponse> CreateMerchant(MerchantCreationRequest request)
        {
            var existingMerchant = await _paymentDb.Merchants.FirstOrDefaultAsync(m => m.Login == request.Login);
            if (existingMerchant != null)
            {
                // TODO: Differentiate response
                return null;
            }
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var saltStr = Convert.ToBase64String(salt);
            var merchant = new Merchant
            {
                Name = request.Name,
                Login = request.Login,
                Salt = saltStr,
                HashedPassword = GetHashedPassword(request.Password, saltStr),
                AcquirerType = request.AcquirerType,
                Active = true,
            };
            _paymentDb.Merchants.Add(merchant);

            await _paymentDb.SaveChangesAsync();

            return new MerchantCreationResponse{ Id = merchant.Id };
        }

        public async Task<AuthenticationResponse> Authenticate(AuthenticationRequest request)
        {
            var merchant = await _paymentDb.Merchants.SingleOrDefaultAsync(x => x.Login == request.Login);
            if (merchant == null)
            {
                return null;
            }
            var hashedPassword = GetHashedPassword(request.Password, merchant.Salt);
            if (hashedPassword != merchant.HashedPassword)
            {
                return null;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.JwtSecret);
            var expires = DateTime.UtcNow.AddDays(7);
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

        private static string GetHashedPassword(string password, string salt)
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
