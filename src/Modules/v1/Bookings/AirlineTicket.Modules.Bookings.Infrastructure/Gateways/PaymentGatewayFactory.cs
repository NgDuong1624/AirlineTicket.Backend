using System;
using System.Collections.Generic;
using System.Linq;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Infrastructure.Gateways;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly Dictionary<PaymentProvider, IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways.ToDictionary(g => g.Provider, g => g);
    }

    public IPaymentGateway GetGateway(PaymentProvider provider)
    {
        if (_gateways.TryGetValue(provider, out var gateway))
        {
            return gateway;
        }

        throw new NotSupportedException($"Payment provider '{provider}' is not supported.");
    }
}
