using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Model
{
    public class PaymentRequest
    {
        public string CardNumber { get; set; }
        public byte ExpiryMonth { get; set; }
        public  ushort ExpiryYear { get; set; }
        public ushort CVV { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
    }
}
