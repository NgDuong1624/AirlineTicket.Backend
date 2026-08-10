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

namespace AirlineTicket.Modules.Notifications.Application.UnitTests.GetAllNotifications;

public class GetAllNotificationsQueryHandlerTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<INotificationTemplateService> _templateServiceMock;
    private readonly GetAllNotificationsQueryHandler _handler;

    public GetAllNotificationsQueryHandlerTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _templateServiceMock = new Mock<INotificationTemplateService>();
        _handler = new GetAllNotificationsQueryHandler(_repositoryMock.Object, _templateServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllNotifications()
    {
        // Arrange
        var notifications = new List<Notification>
        {
            new Notification { Id = Guid.NewGuid(), Title = "Static Title", Content = "Static Content" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync(1, 10))
            .ReturnsAsync(notifications);

        // Act
        var result = await _handler.Handle(new GetAllNotificationsQuery(1, 10), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Static Title");
    }
}