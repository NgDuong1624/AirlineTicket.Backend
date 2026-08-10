using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Features.Auth;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Users.Application.UnitTests;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new LogoutCommandHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessAndDoNothing_WhenUserNotFound()
    {
        // Arrange
        var command = new LogoutCommand("non-existent-token");
        _userRepositoryMock.Setup(repo => repo.GetByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRevokeRefreshTokenAndReturnSuccess_WhenUserFound()
    {
        // Arrange
        var command = new LogoutCommand("valid-token");
        var user = new User { RefreshToken = "valid-token", RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1) };

        _userRepositoryMock.Setup(repo => repo.GetByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiryTime.Should().BeNull();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
