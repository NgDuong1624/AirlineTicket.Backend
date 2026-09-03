using AirlineTicket.Modules.Bookings.Domain.Entities;
using AirlineTicket.Modules.Bookings.Domain.Enums;
using AirlineTicket.Modules.Bookings.Domain.ValueObjects;
using Xunit;

namespace AirlineTicket.Modules.Bookings.UnitTest.Domain;

public class PaymentDomainUnitTests
{
    [Theory]
    [InlineData(100.50, Currency.USD, 10050)]
    [InlineData(100.00, Currency.USD, 10000)]
    [InlineData(250000, Currency.VND, 250000)]
    [InlineData(5000, Currency.JPY, 5000)]
    [InlineData(12000, Currency.KRW, 12000)]
    [InlineData(89.99, Currency.EUR, 8999)]
    public void Money_ToGatewayUnits_CalculatesCorrectUnits(decimal amount, Currency currency, long expectedUnits)
    {
        var money = new Money(amount, currency);
        Assert.Equal(expectedUnits, money.ToGatewayUnits());
    }

    [Theory]
    [InlineData(Currency.VND)]
    [InlineData(Currency.JPY)]
    [InlineData(Currency.KRW)]
    public void Money_ZeroDecimalCurrencies_RejectFractionalAmounts(Currency currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(100.50m, currency));
    }

    [Fact]
    public void Money_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-10m, Currency.USD));
    }

    [Fact]
    public void Payment_CanTransitionTo_ValidForwardTransitions()
    {
        var payment = new Payment { Status = PaymentTransactionStatus.Pending };

        Assert.True(payment.CanTransitionTo(PaymentTransactionStatus.Processing));
        Assert.True(payment.CanTransitionTo(PaymentTransactionStatus.Succeeded));
        Assert.True(payment.CanTransitionTo(PaymentTransactionStatus.Failed));
        Assert.True(payment.CanTransitionTo(PaymentTransactionStatus.Cancelled));

        payment.TransitionTo(PaymentTransactionStatus.Succeeded);
        Assert.True(payment.CanTransitionTo(PaymentTransactionStatus.RefundPending));
        Assert.True(payment.CanTransitionTo(PaymentTransactionStatus.Refunded));
    }

    [Fact]
    public void Payment_TransitionTo_TerminalStates_RejectsFurtherTransitions()
    {
        var payment = new Payment { Status = PaymentTransactionStatus.Failed };
        Assert.False(payment.CanTransitionTo(PaymentTransactionStatus.Succeeded));
        Assert.Throws<InvalidOperationException>(() => payment.TransitionTo(PaymentTransactionStatus.Succeeded));
    }

    [Fact]
    public void Payment_TransitionTo_StaleTimestamp_DropsUpdate()
    {
        var now = DateTime.UtcNow;
        var payment = new Payment
        {
            Status = PaymentTransactionStatus.Pending,
            ProviderTimestamp = now
        };

        // Out-of-order older event
        payment.TransitionTo(PaymentTransactionStatus.Processing, providerTimestamp: now.AddMinutes(-5));

        // State remains Pending
        Assert.Equal(PaymentTransactionStatus.Pending, payment.Status);
    }
}
