using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Partner.Aircraft;

[Collection("FlightsTests")]
public class CreatePartnerAircraftTests : BaseIntegrationTest
{
    public CreatePartnerAircraftTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePartnerAircraft_ShouldReturnBadRequest_WhenModelDoesNotExist()
    {
        // Arrange
        var airlineId = Guid.NewGuid();

        // Act
        var response = await Client.AsPartner(airlineId).PostAsync("/api/partner/aircraft", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/partner/aircraft: CreatePartnerAircraft_ShouldReturnBadRequest_WhenModelDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
