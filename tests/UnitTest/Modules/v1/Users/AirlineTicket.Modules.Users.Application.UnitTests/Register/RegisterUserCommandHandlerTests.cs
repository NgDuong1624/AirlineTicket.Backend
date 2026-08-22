using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.BuildingBlocks.Application.Localization;
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

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ILanguageResolver> _languageResolverMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _languageResolverMock = new Mock<ILanguageResolver>();
        _languageResolverMock.Setup(x => x.ResolveLanguage()).Returns("en");
        _languageResolverMock.Setup(x => x.NormalizeCulture(It.IsAny<string>())).Returns<string>(c => c.ToLowerInvariant());
        _handler = new RegisterUserCommandHandler(_userRepositoryMock.Object, _passwordHasherMock.Object, _languageResolverMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand("test@test.com", "password123", "Test User", "123456789");
        _userRepositoryMock.Setup(repo => repo.IsEmailUniqueAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Not unique

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailAlreadyExists");
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserId_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var command = new RegisterUserCommand("new@test.com", "password123", "New User", "123456789");
        _userRepositoryMock.Setup(repo => repo.IsEmailUniqueAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _passwordHasherMock.Setup(hasher => hasher.HashPassword(command.Password))
            .Returns("hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u =>
            u.Email == command.Email &&
            u.PasswordHash == "hashed_password" &&
            u.FullName == command.FullName),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
