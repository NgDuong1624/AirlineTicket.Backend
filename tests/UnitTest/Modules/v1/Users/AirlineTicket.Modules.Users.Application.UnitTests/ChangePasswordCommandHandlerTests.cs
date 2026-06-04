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

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _handler = new ChangePasswordCommandHandler(_userRepositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserNotFound()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "old-password", "new-password");
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User not found.");
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "wrong-old-password", "new-password");
        var user = new User { Id = command.UserId, PasswordHash = "hashed-old-password" };
        
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _passwordHasherMock.Setup(h => h.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Incorrect current password.");
    }

    [Fact]
    public async Task Handle_ShouldUpdatePassword_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "correct-old-password", "new-password");
        var user = new User { Id = command.UserId, PasswordHash = "hashed-old-password" };
        var newHashedPassword = "hashed-new-password";

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
            
        _passwordHasherMock.Setup(h => h.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            .Returns(true);
            
        _passwordHasherMock.Setup(h => h.HashPassword(command.NewPassword))
            .Returns(newHashedPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        user.PasswordHash.Should().Be(newHashedPassword);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
