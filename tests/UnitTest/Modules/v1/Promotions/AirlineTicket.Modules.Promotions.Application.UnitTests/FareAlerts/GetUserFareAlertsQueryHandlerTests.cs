using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Application.Features.Public;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Application.UnitTests.FareAlerts;

public class GetUserFareAlertsQueryHandlerTests
{
    private readonly Mock<IFareAlertRepository> _repositoryMock;
    private readonly GetUserFareAlertsQueryHandler _handler;

    public GetUserFareAlertsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IFareAlertRepository>();
        _handler = new GetUserFareAlertsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserAlerts_WhenAlertsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var alertId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var destId = Guid.NewGuid();
        var departureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

        var alerts = new List<FareAlert>
        {
            new()
            {
                Id = alertId,
                UserId = userId,
                OriginAirportId = originId,
                DestinationAirportId = destId,
                DepartureDate = departureDate,
                ReturnDate = null,
                TargetPrice = 1200000m,
                CurrentLowestPrice = 1100000m,
                LastNotifiedPrice = 1100000m,
                Currency = "VND",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _repositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerts);

        var query = new GetUserFareAlertsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var dto = result.Value[0];
        dto.Id.Should().Be(alertId);
        dto.UserId.Should().Be(userId);
        dto.OriginAirportId.Should().Be(originId);
        dto.DestinationAirportId.Should().Be(destId);
        dto.TargetPrice.Should().Be(1200000m);
        dto.CurrentLowestPrice.Should().Be(1100000m);
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAlertsExistForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FareAlert>());

        var query = new GetUserFareAlertsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
