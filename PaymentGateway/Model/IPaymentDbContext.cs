using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PaymentGateway.Model
{
    public interface IPaymentDbContext
    {
        DbSet<Payment> Payments { get; set; }
        DbSet<Merchant> Merchants { get; set; }
        DatabaseFacade Database { get;  }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}