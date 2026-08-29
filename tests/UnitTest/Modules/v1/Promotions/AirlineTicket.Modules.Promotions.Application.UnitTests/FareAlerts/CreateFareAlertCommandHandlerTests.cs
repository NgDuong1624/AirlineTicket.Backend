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

public class CreateFareAlertCommandHandlerTests
{
    private readonly Mock<IFareAlertRepository> _repositoryMock;
    private readonly CreateFareAlertCommandHandler _handler;

    public CreateFareAlertCommandHandlerTests()
    {
        _repositoryMock = new Mock<IFareAlertRepository>();
        _handler = new CreateFareAlertCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var destId = Guid.NewGuid();
        var departureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var expectedId = Guid.NewGuid();

        var command = new CreateFareAlertCommand(
            userId,
            originId,
            destId,
            departureDate,
            departureDate.AddDays(3),
            1500000m,
            2000000m,
            "VND");

        _repositoryMock.Setup(r => r.GetUserAlertForRouteAsync(userId, originId, destId, departureDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FareAlert?)null);

        _repositoryMock.Setup(r => r.CountActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<FareAlert>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedId);

        _repositoryMock.Verify(r => r.CreateAsync(
            It.Is<FareAlert>(a =>
                a.UserId == userId &&
                a.OriginAirportId == originId &&
                a.DestinationAirportId == destId &&
                a.DepartureDate == departureDate &&
                a.TargetPrice == 1500000m &&
                a.CurrentLowestPrice == 2000000m &&
                a.Currency == "VND" &&
                a.IsActive == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDuplicateAlertExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var destId = Guid.NewGuid();
        var departureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        var command = new CreateFareAlertCommand(
            userId,
            originId,
            destId,
            departureDate,
            null,
            1000000m,
            1200000m);

        _repositoryMock.Setup(r => r.GetUserAlertForRouteAsync(userId, originId, destId, departureDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FareAlert { Id = Guid.NewGuid(), UserId = userId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FareAlert.Duplicate");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<FareAlert>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenMaxActiveAlertsLimitExceeded()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var originId = Guid.NewGuid();
        var destId = Guid.NewGuid();
        var departureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));

        var command = new CreateFareAlertCommand(
            userId,
            originId,
            destId,
            departureDate,
            null,
            1000000m,
            1500000m);

        _repositoryMock.Setup(r => r.GetUserAlertForRouteAsync(userId, originId, destId, departureDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FareAlert?)null);

        _repositoryMock.Setup(r => r.CountActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FareAlert.LimitExceeded");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<FareAlert>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Validator_ShouldFail_WhenOriginEqualsDestination()
    {
        // Arrange
        var airportId = Guid.NewGuid();
        var command = new CreateFareAlertCommand(
            Guid.NewGuid(),
            airportId,
            airportId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            null,
            500000m,
            600000m);

        var validator = new CreateFareAlertCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFareAlertCommand.OriginAirportId));
    }

    [Fact]
    public void Validator_ShouldFail_WhenDepartureDateIsInPast()
    {
        // Arrange
        var command = new CreateFareAlertCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            null,
            500000m,
            600000m);

        var validator = new CreateFareAlertCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFareAlertCommand.DepartureDate));
    }

    [Fact]
    public void Validator_ShouldFail_WhenReturnDateIsEarlierThanDepartureDate()
    {
        // Arrange
        var departureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var command = new CreateFareAlertCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            departureDate,
            departureDate.AddDays(-1),
            500000m,
            600000m);

        var validator = new CreateFareAlertCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFareAlertCommand.ReturnDate));
    }

    [Fact]
    public void Validator_ShouldFail_WhenTargetPriceIsZeroOrNegative()
    {
        // Arrange
        var command = new CreateFareAlertCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            null,
            0m,
            600000m);

        var validator = new CreateFareAlertCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateFareAlertCommand.TargetPrice));
    }
}
