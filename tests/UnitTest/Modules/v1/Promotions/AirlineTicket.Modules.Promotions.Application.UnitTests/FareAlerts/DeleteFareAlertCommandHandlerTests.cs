using System;
using System.Threading;
using System.Threading.Tasks;
using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Application.Features.Public;
using FluentAssertions;
using Moq;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Application.UnitTests.FareAlerts;

public class DeleteFareAlertCommandHandlerTests
{
    private readonly Mock<IFareAlertRepository> _repositoryMock;
    private readonly DeleteFareAlertCommandHandler _handler;

    public DeleteFareAlertCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFareAlertRepository>();
        _handler = new DeleteFareAlertCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAlertExistsAndDeleted()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteFareAlertCommand(alertId, userId);

        _repositoryMock.Setup(r => r.DeleteAsync(alertId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(alertId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAlertNotFoundOrNotOwned()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteFareAlertCommand(alertId, userId);

        _repositoryMock.Setup(r => r.DeleteAsync(alertId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FareAlert.NotFound");
    }

    [Fact]
    public void Validator_ShouldFail_WhenIdOrUserIdIsEmpty()
    {
        // Arrange
        var command = new DeleteFareAlertCommand(Guid.Empty, Guid.Empty);
        var validator = new DeleteFareAlertCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteFareAlertCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteFareAlertCommand.UserId));
    }
}
