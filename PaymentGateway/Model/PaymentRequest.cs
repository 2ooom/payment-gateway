using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Model
{
    public class PaymentRequest
    {
        [Required]
        public string CardNumber { get; set; }

        [Required]
        [Range(1, 12)]
        public byte ExpiryMonth { get; set; }

        [Required]
        [Range(2020, int.MaxValue)]
        public ushort ExpiryYear { get; set; }

        [Required]
        public string Cvv { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; }
    }
}
