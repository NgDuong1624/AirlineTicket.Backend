using AirlineTicket.Modules.Interactions.Application.Features.Qa;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Interactions.Application.UnitTests.GetAirTicketAnswer;

public class GetAirTicketAnswerQueryHandlerTests
{
    private readonly Mock<IAirTicketAiClient> _aiClientMock;
    private readonly GetAirTicketAnswerQueryHandler _handler;

    public GetAirTicketAnswerQueryHandlerTests()
    {
        _aiClientMock = new Mock<IAirTicketAiClient>();
        _handler = new GetAirTicketAnswerQueryHandler(_aiClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAnswer_WhenQuestionIsValid()
    {
        // Arrange
        var question = "How much is a ticket to Paris?";
        var aiResult = new AirTicketAiResult("It costs $500.", "Reasoning...", "gpt-4");

        _aiClientMock.Setup(c => c.AskAsync(question, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiResult);

        // Act
        var result = await _handler.Handle(new GetAirTicketAnswerQuery(question), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Answer.Should().Be("It costs $500.");
        result.Reasoning.Should().Be("Reasoning...");
        result.Model.Should().Be("gpt-4");
        result.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Handle_ShouldThrowArgumentException_WhenQuestionIsEmpty()
    {
        // Arrange
        var question = "";

        // Act
        Func<Task> act = async () => await _handler.Handle(new GetAirTicketAnswerQuery(question), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Question cannot be empty. (Parameter 'request')");
    }
}