using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Flights.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Flights.Api.IntegrationTests.Endpoints.Admin.AircraftModels.Id;

[Collection("FlightsTests")]
public class AdminUpdateAircraftModelTests : BaseIntegrationTest
{
    public AdminUpdateAircraftModelTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AdminUpdateModel_ShouldReturnBadRequest_WhenModelDoesNotExist()
    {
        // Arrange
        var request = new AdminAircraftModelRequest(
            Name: "787-9",
            Manufacturer: "Boeing",
            TotalSeats: 290,
            SeatTemplates: new List<AirlineTicket.Modules.Flights.Application.Features.AircraftModels.SeatTemplateDto>());

        // Act
        var response = await Client.AsAdmin().PutAsJsonAsync($"/api/admin/aircraft-models/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "PUT /api/admin/aircraft-models/{Guid.NewGuid()}: AdminUpdateModel_ShouldReturnBadRequest_WhenModelDoesNotExist must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
