﻿using System.ComponentModel.DataAnnotations;

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

        public string HashedPassword { get; set; }

        public string Salt { get; set; }

        public AcquirerType AcquirerType { get; set; }
    }
}
