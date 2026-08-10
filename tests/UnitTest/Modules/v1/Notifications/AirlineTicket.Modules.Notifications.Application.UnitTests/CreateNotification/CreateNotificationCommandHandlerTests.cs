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

namespace AirlineTicket.Modules.Notifications.Application.UnitTests.CreateNotification;

public class CreateNotificationCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _repositoryMock;
    private readonly Mock<INotificationTemplateService> _templateServiceMock;
    private readonly Mock<INotificationPusher> _notificationPusherMock;
    private readonly CreateNotificationCommandHandler _handler;

    public CreateNotificationCommandHandlerTests()
    {
        _repositoryMock = new Mock<INotificationRepository>();
        _templateServiceMock = new Mock<INotificationTemplateService>();
        _notificationPusherMock = new Mock<INotificationPusher>();

        _handler = new CreateNotificationCommandHandler(
            _repositoryMock.Object,
            _templateServiceMock.Object,
            _notificationPusherMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateNotification_WhenTemplateExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateNotificationCommand(
            userId,
            "WELCOME_TEMPLATE",
            new Dictionary<string, string> { { "Name", "User" } },
            1,
            null,
            null,
            null);

        var template = new AirlineTicket.Modules.Notifications.Domain.Entities.NotificationTemplate
        {
            Subject = "Welcome, {Name}!",
            BodyTemplate = "Hello, {Name}!"
        };

        _templateServiceMock.Setup(s => s.GetTemplateAsync("WELCOME_TEMPLATE", "en"))
            .ReturnsAsync(template);

        _templateServiceMock.Setup(s => s.RenderTemplate(template.Subject, command.TemplateParameters))
            .Returns("Welcome, User!");

        _templateServiceMock.Setup(s => s.RenderTemplate(template.BodyTemplate, command.TemplateParameters))
            .Returns("Hello, User!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Notification>(n => n.Title == "Welcome, User!" && n.Content == "Hello, User!")), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _notificationPusherMock.Verify(p => p.PushNotificationAsync(userId, It.IsAny<AirlineTicket.Modules.Notifications.Application.DTOs.NotificationDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}