using System.Collections.Generic;
using PaymentGateway.Model;

namespace PaymentGateway.Acquiring
{
    public class BankRegistry : IBankRegistry
    {
        private readonly Dictionary<long, IAcquirer> _registry;

        public BankRegistry()
        {
            _registry = new Dictionary<long, IAcquirer>();
        }

        public IAcquirer GetAcquirer(Merchant merchant)
        {
            if (_registry.TryGetValue(merchant.Id, out var acquirer))
            {
                return acquirer;
            }
            acquirer = merchant.AcquirerType switch
            {
                // TODO: implement other Acquirers
                _ => new DummyAcquirer(merchant)
            };
            _registry.Add(merchant.Id, acquirer);

            return acquirer;
        }
    }
}
