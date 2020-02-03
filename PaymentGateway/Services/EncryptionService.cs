using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace PaymentGateway.Services
{
    public class EncryptionService : IEncryptionService
    {
        private const int NbHashIterations = 1000;

        public string GetHash(string toHash, string salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: toHash,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: NbHashIterations,
                numBytesRequested: 256 / 8));
        }

        public string GenerateSalt()
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return Convert.ToBase64String(salt);
        }

    }
}
