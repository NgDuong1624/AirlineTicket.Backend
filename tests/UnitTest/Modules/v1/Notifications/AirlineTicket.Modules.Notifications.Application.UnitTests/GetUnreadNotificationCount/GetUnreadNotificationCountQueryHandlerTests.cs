using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Application.Features.Queries;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Application.UnitTests.GetUnreadNotificationCount;

public class GetUnreadNotificationCountQueryHandlerTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly GetUnreadNotificationCountQueryHandler _handler;

    public GetUnreadNotificationCountQueryHandlerTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _handler = new GetUnreadNotificationCountQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnreadCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetUnreadCountByUserIdAsync(userId))
            .ReturnsAsync(5);

        // Act
        var result = await _handler.Handle(new GetUnreadNotificationCountQuery(userId), CancellationToken.None);

        // Assert
        result.Should().Be(5);
    }
}