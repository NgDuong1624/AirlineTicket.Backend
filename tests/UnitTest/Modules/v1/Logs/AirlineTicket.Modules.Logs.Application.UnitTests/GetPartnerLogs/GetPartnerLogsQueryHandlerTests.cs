using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Logs.Application.Contracts;
using AirlineTicket.Modules.Logs.Application.Features;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Logs.Application.UnitTests.GetPartnerLogs;

public class GetPartnerLogsQueryHandlerTests
{
    private readonly Mock<ILogRepository> _logRepositoryMock;
    private readonly GetPartnerLogsQueryHandler _handler;

    public GetPartnerLogsQueryHandlerTests()
    {
        _logRepositoryMock = new Mock<ILogRepository>();
        _handler = new GetPartnerLogsQueryHandler(_logRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedLogsForPartner()
    {
        // Arrange
        var airlineId = Guid.NewGuid();
        var logs = new List<LogDto>
        {
            new LogDto { Id = Guid.NewGuid(), Level = "Info", Message = "Flight created", CreatedAt = DateTime.UtcNow, AirlineId = airlineId, IsSystemLog = false }
        };
        var pagedResult = PagedResult<LogDto>.Success(logs, 1, 10, 1);

        _logRepositoryMock.Setup(r => r.GetAirlineLogsAsync(1, 10, airlineId, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(new GetPartnerLogsQuery(airlineId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }
}