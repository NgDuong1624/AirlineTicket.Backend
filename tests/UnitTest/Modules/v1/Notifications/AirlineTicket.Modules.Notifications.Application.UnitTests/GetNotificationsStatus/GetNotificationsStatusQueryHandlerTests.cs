using AirlineTicket.Modules.Notifications.Application.Features.Status;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Notifications.Application.UnitTests.GetNotificationsStatus;

public class GetNotificationsStatusQueryHandlerTests
{
    private readonly GetNotificationsStatusQueryHandler _handler;

    public GetNotificationsStatusQueryHandlerTests()
    {
        _handler = new GetNotificationsStatusQueryHandler();
    }

    [Fact]
    public async Task Handle_ShouldReturnOkStatus()
    {
        // Act
        var result = await _handler.Handle(new GetNotificationsStatusQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Notifications Module OK");
    }
}