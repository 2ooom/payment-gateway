using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Model
{
    public class Payment
    {
        public long Id { get; set; }
        public double Amount { get; set; }
        public string Last4Digits { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
