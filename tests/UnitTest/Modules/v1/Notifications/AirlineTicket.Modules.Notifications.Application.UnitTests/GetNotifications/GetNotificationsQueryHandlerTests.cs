using AirlineTicket.Modules.Notifications.Application.Contracts;
using AirlineTicket.Modules.Notifications.Application.Features.Queries;
using AirlineTicket.Modules.Notifications.Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Application.UnitTests.GetNotifications;

public class GetNotificationsQueryHandlerTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<INotificationTemplateService> _templateServiceMock;
    private readonly GetNotificationsQueryHandler _handler;

    public GetNotificationsQueryHandlerTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _templateServiceMock = new Mock<INotificationTemplateService>();
        _handler = new GetNotificationsQueryHandler(_repositoryMock.Object, _templateServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            new Notification { Id = Guid.NewGuid(), UserId = userId, Title = "Static Title", Content = "Static Content" }
        };

        _repositoryMock.Setup(r => r.GetByUserIdAsync(userId, 1, 10))
            .ReturnsAsync(notifications);

        // Act
        var result = await _handler.Handle(new GetNotificationsQuery(userId, 1, 10), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Static Title");
    }
}