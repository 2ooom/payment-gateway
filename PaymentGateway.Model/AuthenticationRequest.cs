using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Model
{
    public class AuthenticationRequest
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
