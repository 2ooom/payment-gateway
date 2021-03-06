﻿using Microsoft.EntityFrameworkCore;

namespace PaymentGateway.Model
{
    public class PaymentDbContext : DbContext, IPaymentDbContext
    {
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }
    }
}
