using AirlineTicket.Modules.Bookings.Domain.Enums;

namespace AirlineTicket.Modules.Bookings.Domain.ValueObjects;

public readonly record struct Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        }

        if (IsZeroDecimalCurrency(currency) && amount % 1 != 0)
        {
            throw new ArgumentException($"{currency} does not support decimal places.", nameof(amount));
        }

        Amount = amount;
        Currency = currency;
    }

    public static bool IsZeroDecimalCurrency(Currency currency) => currency switch
    {
        Currency.VND => true,
        Currency.JPY => true,
        Currency.KRW => true,
        _ => false
    };

    public long ToGatewayUnits() => IsZeroDecimalCurrency(Currency)
        ? (long)Amount
        : (long)Math.Round(Amount * 100, MidpointRounding.AwayFromZero);

    public static Money FromGatewayUnits(long units, Currency currency) => IsZeroDecimalCurrency(currency)
        ? new Money(units, currency)
        : new Money(units / 100m, currency);
}
