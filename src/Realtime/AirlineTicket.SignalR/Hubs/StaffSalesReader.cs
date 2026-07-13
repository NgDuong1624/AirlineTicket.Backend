using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Bookings.Application.Contracts;
using AirlineTicket.Modules.Flights.Application.Contracts;

namespace AirlineTicket.SignalR.Hubs;

/// <summary>
/// Assembles staff ticket-sales rows by joining the Bookings and Flights data stores.
/// Lives in the host so neither module needs a reference to the other.
/// </summary>
public class StaffSalesReader : IStaffSalesReader
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IFlightSeatRepository _flightSeatRepository;

    public StaffSalesReader(
        IBookingRepository bookingRepository,
        IFlightRepository flightRepository,
        IFlightSeatRepository flightSeatRepository)
    {
        _bookingRepository = bookingRepository;
        _flightRepository = flightRepository;
        _flightSeatRepository = flightSeatRepository;
    }

    public async Task<List<StaffSaleDto>> GetSalesAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingRepository.GetStaffSalesBookingsAsync(cancellationToken);

        var bookingIdsWithTickets = bookings.Where(b => b.FlightId.HasValue).ToList();
        var flightIds = bookingIdsWithTickets.Select(b => b.FlightId!.Value).Distinct().ToList();
        var seatIds = bookingIdsWithTickets.Where(b => b.SeatId.HasValue).Select(b => b.SeatId!.Value).Distinct().ToList();

        var flights = await _flightRepository.GetByIdsAsync(flightIds, cancellationToken);
        var flightsDict = flights.ToDictionary(f => f.Id);

        var seatClasses = await _flightSeatRepository.GetSeatClassesAsync(seatIds, cancellationToken);

        return bookings.Select(b =>
        {
            string flightNumber = string.Empty;
            string route = string.Empty;
            string seatClass = string.Empty;

            if (b.FlightId.HasValue && flightsDict.TryGetValue(b.FlightId.Value, out var f))
            {
                flightNumber = f.FlightNumber;
                route = $"{f.OriginCode} → {f.DestinationCode}";
            }
            if (b.SeatId.HasValue && seatClasses.TryGetValue(b.SeatId.Value, out var sc))
            {
                seatClass = sc;
            }

            return new StaffSaleDto
            {
                Id = b.PnrCode,
                BookingId = b.Id,
                PassengerName = b.PassengerName,
                FlightNumber = flightNumber,
                Route = route,
                SeatClass = seatClass,
                Amount = b.TotalPrice,
                Status = b.Status,
                BookedAt = b.CreatedAt
            };
        }).ToList();
    }
}
