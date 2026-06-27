using System;
using System.Threading.Tasks;

namespace AirlineTicket.Modules.Flights.Application.Contracts;

public interface ISeatGenerationService
{
    Task GenerateSeatsAsync(Guid airplaneId, Guid aircraftModelId);
}
