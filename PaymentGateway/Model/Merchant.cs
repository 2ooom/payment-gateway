using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Model
{
    public class Merchant
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Url { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string HashedPassword { get; set; }

        [Required]
        public string Salt { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public AcquirerType AcquirerType { get; set; }
    }
}
