using AirlineTicket.Modules.Interactions.Application.Features.Status;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Interactions.Application.UnitTests.GetInteractionsStatus;

public class GetInteractionsStatusQueryHandlerTests
{
    private readonly GetInteractionsStatusQueryHandler _handler;

    public GetInteractionsStatusQueryHandlerTests()
    {
        _handler = new GetInteractionsStatusQueryHandler();
    }

    [Fact]
    public async Task Handle_ShouldReturnOkStatus()
    {
        // Act
        var result = await _handler.Handle(new GetInteractionsStatusQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Interactions Module OK");
    }
}