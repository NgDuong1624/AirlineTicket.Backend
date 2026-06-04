using AirlineTicket.Modules.Users.Application.Features.Auth;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Application.Services;
using AirlineTicket.Modules.Users.Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Users.Application.UnitTests;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new RefreshTokenCommandHandler(_userRepositoryMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
    {
        // Arrange
        var command = new RefreshTokenCommand("old-refresh-token");
        _userRepositoryMock.Setup(repo => repo.GetByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenTokenIsExpired()
    {
        // Arrange
        var command = new RefreshTokenCommand("expired-refresh-token");
        var user = new User { RefreshToken = "expired-refresh-token", RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-10) };
        
        _userRepositoryMock.Setup(repo => repo.GetByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task Handle_ShouldReturnNewJwtToken_WhenRefreshTokenIsValid()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid-refresh-token");
        var user = new User { RefreshToken = "valid-refresh-token", RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1) };
        var expectedNewToken = "new-jwt-token";

        _userRepositoryMock.Setup(repo => repo.GetByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _jwtServiceMock.Setup(s => s.GenerateToken(user))
            .Returns(expectedNewToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedNewToken);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        user.RefreshToken.Should().NotBeNullOrEmpty();
        user.RefreshToken.Should().NotBe("valid-refresh-token"); // It should be rotated
    }
}
