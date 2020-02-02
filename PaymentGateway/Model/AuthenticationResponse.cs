using System;

namespace PaymentGateway.Model
{
    public class AuthenticationResponse
    {
        public string JwtToken { get; set; }
        public DateTime Expires { get; set; }
    }
}
