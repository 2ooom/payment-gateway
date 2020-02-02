using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PaymentGateway.Model
{
    public interface IPaymentDbContext
    {
        DbSet<Payment> Payments { get; set; }
        DbSet<Merchant> Merchants { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}