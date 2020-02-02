using Microsoft.Extensions.Configuration;

namespace PaymentGateway
{
    public class Config : IConfig
    {
        public string JwtSecret { get; }
        public string PaymentDbConnectionString { get; }

        public Config(IConfiguration configuration)
        {
            JwtSecret = configuration["JwtSecret"];
            PaymentDbConnectionString = configuration["PaymentDbConnectionString"];
        }
    }
}
