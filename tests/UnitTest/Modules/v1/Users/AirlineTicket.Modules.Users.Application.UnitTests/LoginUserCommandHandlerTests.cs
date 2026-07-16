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
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AirlineTicket.Modules.Users.Application.UnitTests;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _configurationMock = new Mock<IConfiguration>();
        
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(x => x.Value).Returns("2");
        _configurationMock.Setup(x => x.GetSection("Jwt:RefreshTokenExpiryHours")).Returns(configSectionMock.Object);

        _handler = new LoginUserCommandHandler(
            _userRepositoryMock.Object, 
            _passwordHasherMock.Object, 
            _jwtServiceMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var command = new LoginUserCommand("wrong@test.com", "password123");
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UNAUTHORIZED");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsIncorrect()
    {
        // Arrange
        var command = new LoginUserCommand("test@test.com", "wrong_password");
        var user = new User { Email = "test@test.com", PasswordHash = "correct_hash" };

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UNAUTHORIZED");
    }

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUserCommand("test@test.com", "correct_password");
        var user = new User { Email = "test@test.com", PasswordHash = "correct_hash" };
        var expectedToken = "jwt.token.string";

        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(hasher => hasher.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _jwtServiceMock.Setup(jwt => jwt.GenerateToken(user))
            .Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(expectedToken);
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
