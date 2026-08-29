using AirlineTicket.Modules.CMS.Application.Contracts;
using AirlineTicket.Modules.CMS.Application.Features.Dashboard;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.CMS.Application.UnitTests.GetPartnerDashboard;

public class GetPartnerDashboardQueryHandlerTests
{
    private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
    private readonly GetPartnerDashboardQueryHandler _handler;

    public GetPartnerDashboardQueryHandlerTests()
    {
        _dashboardRepositoryMock = new Mock<IDashboardRepository>();
        _handler = new GetPartnerDashboardQueryHandler(_dashboardRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPartnerDashboardData()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var stats = new List<PartnerStatDto> { new PartnerStatDto("100") };
        var recentFlights = new List<PartnerRecentFlightDto> { new PartnerRecentFlightDto("1", "SGN-HAN", DateTime.UtcNow, "Scheduled", "green") };
        var recentBookings = new List<PartnerRecentBookingDto> { new PartnerRecentBookingDto("1", "John Doe", "VN123", "1A", "Economy", 100m, DateTime.UtcNow) };

        _dashboardRepositoryMock.Setup(r => r.GetPartnerStatsAsync(airlineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);
        _dashboardRepositoryMock.Setup(r => r.GetPartnerRecentFlightsAsync(airlineId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentFlights);
        _dashboardRepositoryMock.Setup(r => r.GetPartnerRecentBookingsAsync(airlineId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentBookings);

        // Act
        var result = await _handler.Handle(new GetPartnerDashboardQuery(airlineId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Stats.Should().HaveCount(1);
        result.Value.RecentFlights.Should().HaveCount(1);
        result.Value.RecentBookings.Should().HaveCount(1);
    }
}