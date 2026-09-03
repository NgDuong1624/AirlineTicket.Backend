namespace AirlineTicket.Modules.Bookings.Domain.Enums;

public enum PaymentTransactionStatus
{
    Pending = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5,
    RefundPending = 6,
    Refunded = 7
}
