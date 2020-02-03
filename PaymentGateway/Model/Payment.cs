using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Acquiring;

namespace PaymentGateway.Model
{
    public class Payment
    {
        [Key]
        public  string Id { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; }

        [Required]
        [Range(1, 12)]
        public byte ExpiryMonth { get; set; }

        [Required]
        [Range(2020, int.MaxValue)]
        public ushort ExpiryYear { get; set; }

        [Required]
        public string CardLastDigits { get; set; }

        [Required]
        public string CardNumberHashed { get; set; }

        [Required]
        public byte CardNumberLength { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; }

        [Required]
        [ForeignKey("MerchantId")]
        public long MerchantId { get; set; }
    }
}
