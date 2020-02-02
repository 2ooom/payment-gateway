using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Model
{
    public class Payment
    {
        [Key]
        public long Id { get; set; }

        public double Amount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; }

        public string Last4Digits { get; set; }

        public DateTime Timestamp { get; set; }

        [ForeignKey("MerchantId")]
        public int MerchantId { get; set; }

        public Merchant Merchant { get; set; }
    }
}
