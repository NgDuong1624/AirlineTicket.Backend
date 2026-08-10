using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Application.Features.Commands;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Application.UnitTests.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly MarkNotificationAsReadCommandHandler _handler;

    public MarkNotificationAsReadCommandHandlerTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _handler = new MarkNotificationAsReadCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTrue_WhenNotificationExistsAndBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var notification = new Notification { Id = notificationId, UserId = userId };

        _repositoryMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        // Act
        var result = await _handler.Handle(new MarkNotificationAsReadCommand(notificationId, userId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenNotificationNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync((Notification?)null);

        // Act
        var result = await _handler.Handle(new MarkNotificationAsReadCommand(notificationId, userId), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}