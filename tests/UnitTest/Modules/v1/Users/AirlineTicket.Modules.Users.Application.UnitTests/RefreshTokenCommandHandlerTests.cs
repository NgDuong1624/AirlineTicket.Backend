using AirlineTicket.BuildingBlocks.Responses;
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
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var command = new RefreshTokenCommand("old-refresh-token");
        _userRepositoryMock.Setup(repo => repo.GetByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenIsExpired()
    {
        // Arrange
        var command = new RefreshTokenCommand("expired-refresh-token");
        var user = new User { RefreshToken = "expired-refresh-token", RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-10) };

        _userRepositoryMock.Setup(repo => repo.GetByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task Handle_ShouldReturnNewJwtToken_WhenRefreshTokenIsValid()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid-refresh-token");
        var user = new User { RefreshToken = "valid-refresh-token", RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1) };
        var expectedNewToken = "new-jwt-token";
        var expectedRefresh = "new-refresh-token";

        _userRepositoryMock.Setup(repo => repo.GetByRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtServiceMock.Setup(s => s.GenerateToken(user))
            .Returns(new TokenResponse(expectedNewToken, expectedRefresh));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(expectedNewToken);
        result.Value.RefreshToken.Should().Be(expectedRefresh);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
