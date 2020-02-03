namespace PaymentGateway.Services
{
    public interface IEncryptionService
    {
        string GetHash(string toHash, string salt);
        string GenerateSalt();
    }
}