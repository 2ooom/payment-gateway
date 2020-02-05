using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Model
{
    public class MerchantCreationRequest
    {
        [Required]
        public string Name { get; set; }

        public string Url { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public AcquirerType AcquirerType { get; set; }
    }
}
