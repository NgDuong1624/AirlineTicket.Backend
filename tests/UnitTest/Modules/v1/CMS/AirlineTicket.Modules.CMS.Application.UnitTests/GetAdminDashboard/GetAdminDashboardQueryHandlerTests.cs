using AirlineTicket.Modules.CMS.Application.Contracts;
using AirlineTicket.Modules.CMS.Application.Features.Dashboard;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.CMS.Application.UnitTests.GetAdminDashboard;

public class GetAdminDashboardQueryHandlerTests
{
    private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
    private readonly GetAdminDashboardQueryHandler _handler;

    public GetAdminDashboardQueryHandlerTests()
    {
        _dashboardRepositoryMock = new Mock<IDashboardRepository>();
        _handler = new GetAdminDashboardQueryHandler(_dashboardRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAdminDashboardData()
    {
        // Arrange
        var statsData = new AdminDashboardStatsDto(10000, 50, 10, 20);
        var partners = new List<AdminDashboardPartnerDto> { new AdminDashboardPartnerDto("Partner 1", "P1", 10, "Active", "2023-01-01") };
        var logs = new List<AdminDashboardLogDto> { new AdminDashboardLogDto("10:00", "Error", "System Error") };

        _dashboardRepositoryMock.Setup(r => r.GetAdminStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(statsData);
        _dashboardRepositoryMock.Setup(r => r.GetRecentPartnersAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partners);
        _dashboardRepositoryMock.Setup(r => r.GetCriticalLogsAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _handler.Handle(new GetAdminDashboardQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Stats.Should().HaveCount(4);
        result.Value.RecentPartners.Should().HaveCount(1);
        result.Value.CriticalLogs.Should().HaveCount(1);
    }
}