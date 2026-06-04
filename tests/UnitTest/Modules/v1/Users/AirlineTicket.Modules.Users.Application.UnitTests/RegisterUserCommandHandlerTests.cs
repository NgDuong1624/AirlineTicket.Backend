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
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _handler = new RegisterUserCommandHandler(_userRepositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand("test@test.com", "password123", "Test User", "123456789");
        _userRepositoryMock.Setup(repo => repo.IsEmailUniqueAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Not unique

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Email already exists");
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
        result.Should().NotBeEmpty();
        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u => 
            u.Email == command.Email && 
            u.PasswordHash == "hashed_password" && 
            u.FullName == command.FullName), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
