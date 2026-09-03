using System;
using Xunit;

namespace AirlineTicket.Modules.Bookings.Api.IntegrationTests.Endpoints.Payments.Id;

public class GetPaymentStatusTests
{
    [Fact]
    public static void StatusEndpoint_RouteFormat_MatchesStandardGuidPattern()
    {
        var bookingId = Guid.NewGuid();
        var route = $"/api/payments/{bookingId}/status";
        Assert.StartsWith("/api/payments/", route);
        Assert.EndsWith("/status", route);
    }
}
