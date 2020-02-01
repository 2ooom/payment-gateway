using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PaymentGateway.Model
{
    public class PaymentDbContext : DbContext
    {
        private string _connectionString { get; }
        public DbSet<Payment> Payments { get; set; }

        public PaymentDbContext(string connectionString = "Data Source=payment.db")
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(_connectionString);
        }
    }
}
