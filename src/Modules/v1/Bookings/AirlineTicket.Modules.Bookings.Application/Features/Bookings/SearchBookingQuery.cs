using AirlineTicket.BuildingBlocks.CQRS;
using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Bookings.Application.Contracts;

namespace AirlineTicket.Modules.Bookings.Application.Features.Bookings;

public record SearchBookingQuery(string PnrCode) : IQuery<Result<BookingDetailDto>>;
