using AirlineTicket.BuildingBlocks.Responses;
using AirlineTicket.Modules.Users.Application.Features.Users;
using AirlineTicket.Modules.Users.Application.Repositories;
using AirlineTicket.Modules.Users.Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Users.Application.UnitTests;

public class GetUserProfileQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetUserProfileQueryHandler _handler;

    public GetUserProfileQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetUserProfileQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var query = new GetUserProfileQuery(Guid.NewGuid());
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(query.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ShouldReturnUserDto_WhenUserFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Test User",
            Phone = "123456789",
            Role = (int)Domain.Enums.UserRole.Customer
        };
        var query = new GetUserProfileQuery(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(query.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.Email.Should().Be("test@example.com");
        result.Value.FullName.Should().Be("Test User");
        result.Value.Role.Should().Be("Customer");
    }
}
