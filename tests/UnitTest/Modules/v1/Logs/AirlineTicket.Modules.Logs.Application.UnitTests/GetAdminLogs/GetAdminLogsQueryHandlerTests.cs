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

namespace AirlineTicket.Modules.Logs.Application.UnitTests.GetAdminLogs;

public class GetAdminLogsQueryHandlerTests
{
    private readonly Mock<ILogRepository> _logRepositoryMock;
    private readonly GetAdminLogsQueryHandler _handler;

    public GetAdminLogsQueryHandlerTests()
    {
        _logRepositoryMock = new Mock<ILogRepository>();
        _handler = new GetAdminLogsQueryHandler(_logRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedLogs()
    {
        // Arrange
        var logs = new List<LogDto>
        {
            new LogDto { Id = Guid.NewGuid(), Level = "Info", Message = "System started", CreatedAt = DateTime.UtcNow, AirlineId = null, IsSystemLog = true }
        };
        var pagedResult = PagedResult<LogDto>.Success(logs, 1, 10, 1);

        _logRepositoryMock.Setup(r => r.GetLogsAsync(1, 10, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(new GetAdminLogsQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }
}