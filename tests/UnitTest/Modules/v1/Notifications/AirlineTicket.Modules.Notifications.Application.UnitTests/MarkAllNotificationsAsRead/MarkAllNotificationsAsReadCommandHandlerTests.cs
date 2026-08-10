using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Application.Features.Commands;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Application.UnitTests.MarkAllNotificationsAsRead;

public class MarkAllNotificationsAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly MarkAllNotificationsAsReadCommandHandler _handler;

    public MarkAllNotificationsAsReadCommandHandlerTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _handler = new MarkAllNotificationsAsReadCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldMarkAllAsRead_WhenUnreadNotificationsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            new Notification { Id = Guid.NewGuid(), UserId = userId, IsRead = false },
            new Notification { Id = Guid.NewGuid(), UserId = userId, IsRead = false }
        };

        _repositoryMock.Setup(r => r.GetByUserIdAsync(userId, 1, 1000))
            .ReturnsAsync(notifications);

        // Act
        var result = await _handler.Handle(new MarkAllNotificationsAsReadCommand(userId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        notifications.Should().OnlyContain(n => n.IsRead);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}