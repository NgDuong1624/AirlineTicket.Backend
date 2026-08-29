using System;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Application.UnitTests.FareAlerts;

public class FareAlertEntityTests
{
    [Fact]
    public void FareAlert_ShouldInitializeWithDefaultValues()
    {
        // Act
        var alert = new FareAlert();

        // Assert
        alert.Currency.Should().Be("VND");
        alert.IsActive.Should().BeTrue();
        alert.LastNotifiedPrice.Should().BeNull();
        alert.LastNotifiedAt.Should().BeNull();
        alert.ReturnDate.Should().BeNull();
    }

    [Fact]
    public void FareAlert_ShouldSetAndRetrievePropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var destId = Guid.NewGuid();
        var departureDate = new DateOnly(2026, 9, 15);
        var returnDate = new DateOnly(2026, 9, 20);

        // Act
        var alert = new FareAlert
        {
            Id = id,
            UserId = userId,
            OriginAirportId = originId,
            DestinationAirportId = destId,
            DepartureDate = departureDate,
            ReturnDate = returnDate,
            TargetPrice = 1500000m,
            CurrentLowestPrice = 1400000m,
            LastNotifiedPrice = 1400000m,
            Currency = "USD",
            IsActive = false,
            LastCheckedAt = DateTime.UtcNow,
            LastNotifiedAt = DateTime.UtcNow
        };

        // Assert
        alert.Id.Should().Be(id);
        alert.UserId.Should().Be(userId);
        alert.OriginAirportId.Should().Be(originId);
        alert.DestinationAirportId.Should().Be(destId);
        alert.DepartureDate.Should().Be(departureDate);
        alert.ReturnDate.Should().Be(returnDate);
        alert.TargetPrice.Should().Be(1500000m);
        alert.CurrentLowestPrice.Should().Be(1400000m);
        alert.LastNotifiedPrice.Should().Be(1400000m);
        alert.Currency.Should().Be("USD");
        alert.IsActive.Should().BeFalse();
        alert.LastNotifiedAt.Should().NotBeNull();
    }
}
