using System.Collections.Generic;
using PaymentGateway.Acquiring;
using PaymentGateway.Model;

namespace PaymentGateway.ITests
{
    public class BankRegistryMock : IBankRegistry
    {
        private static readonly Dictionary<long, IAcquirer> Registry = new Dictionary<long, IAcquirer>();

        public static void SetAcquirer(long merchantId, IAcquirer acquirer)
        {
            Registry[merchantId] = acquirer;
        }

        public IAcquirer GetAcquirer(Merchant merchant)
        {
            return Registry[merchant.Id];
        }
    }
}
