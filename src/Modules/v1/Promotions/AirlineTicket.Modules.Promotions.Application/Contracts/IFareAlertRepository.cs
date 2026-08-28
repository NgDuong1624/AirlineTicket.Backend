using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Promotions.Domain.Entities;

namespace AirlineTicket.Modules.Promotions.Application.Contracts;

public interface IFareAlertRepository
{
    Task<FareAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<FareAlert>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FareAlert?> GetUserAlertForRouteAsync(Guid userId, Guid originAirportId, Guid destinationAirportId, DateOnly departureDate, CancellationToken cancellationToken = default);
    Task<int> CountActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<FareAlert>> GetActiveAlertsAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(FareAlert alert, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(FareAlert alert, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
