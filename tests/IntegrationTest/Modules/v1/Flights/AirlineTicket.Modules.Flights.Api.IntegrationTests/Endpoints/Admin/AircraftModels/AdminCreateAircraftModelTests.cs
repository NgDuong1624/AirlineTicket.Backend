using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.AircraftModels;

[Collection("FlightsTests")]
public class AdminCreateAircraftModelTests : BaseIntegrationTest
{
    public AdminCreateAircraftModelTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminCreateAircraftModel_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var request = new AdminAircraftModelRequest(
            Name: $"A350-{Guid.NewGuid():N}",
            Manufacturer: "Airbus",
            TotalSeats: 300,
            SeatTemplates: new List<AirlineTicket.Modules.Flights.Application.Features.AircraftModels.SeatTemplateDto>());

        // Act
        var response = await Client.AsAdmin().PostAsJsonAsync("/api/admin/aircraft-models", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "POST /api/admin/aircraft-models: AdminCreateAircraftModel_ShouldSucceed_WhenAdmin must return 201 Created but return {0}", response.StatusCode);
    }
}
