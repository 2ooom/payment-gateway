using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Model
{
    public class Merchant
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
    }
}
