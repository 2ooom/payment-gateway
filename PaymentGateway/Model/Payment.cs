using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Acquiring;

namespace PaymentGateway.Model
{
    public class Payment
    {
        [Key]
        public long Id { get; set; }

        public  string AcquirerId { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; }

        [Required]
        public string CardLastDigits { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

        [Required]
        [ForeignKey("MerchantId")]
        public long MerchantId { get; set; }
    }
}
