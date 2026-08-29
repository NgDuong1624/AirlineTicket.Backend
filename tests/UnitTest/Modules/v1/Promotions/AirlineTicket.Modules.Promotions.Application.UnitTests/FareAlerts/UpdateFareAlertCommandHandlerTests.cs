using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Application.Features.Public;
using AirlineTicket.Modules.Promotions.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Application.UnitTests.FareAlerts;

public class UpdateFareAlertCommandHandlerTests
{
    private readonly Mock<IFareAlertRepository> _repositoryMock;
    private readonly UpdateFareAlertCommandHandler _handler;

    public UpdateFareAlertCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFareAlertRepository>();
        _handler = new UpdateFareAlertCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAlertExistsAndUserMatches()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingAlert = new FareAlert
        {
            Id = alertId,
            UserId = userId,
            TargetPrice = 2000000m,
            IsActive = true
        };

        var command = new UpdateFareAlertCommand(alertId, userId, 1800000m, false);

        _repositoryMock.Setup(r => r.GetByIdAsync(alertId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAlert);

        _repositoryMock.Setup(r => r.UpdateAsync(existingAlert, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        existingAlert.TargetPrice.Should().Be(1800000m);
        existingAlert.IsActive.Should().BeFalse();

        _repositoryMock.Verify(r => r.UpdateAsync(existingAlert, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAlertNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new UpdateFareAlertCommand(alertId, userId, 1500000m, true);

        _repositoryMock.Setup(r => r.GetByIdAsync(alertId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FareAlert?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FareAlert.NotFound");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FareAlert>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotOwnAlert()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var existingAlert = new FareAlert
        {
            Id = alertId,
            UserId = ownerId,
            TargetPrice = 2000000m,
            IsActive = true
        };

        var command = new UpdateFareAlertCommand(alertId, attackerId, 1000000m, false);

        _repositoryMock.Setup(r => r.GetByIdAsync(alertId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAlert);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FareAlert.NotFound");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FareAlert>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Validator_ShouldFail_WhenTargetPriceIsZeroOrNegative()
    {
        // Arrange
        var command = new UpdateFareAlertCommand(Guid.NewGuid(), Guid.NewGuid(), 0m, null);
        var validator = new UpdateFareAlertCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateFareAlertCommand.TargetPrice));
    }
}
