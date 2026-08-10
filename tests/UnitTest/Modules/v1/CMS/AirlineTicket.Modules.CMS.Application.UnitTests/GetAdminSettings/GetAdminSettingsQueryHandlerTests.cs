using AirlineTicket.Modules.CMS.Application.Features.Settings;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.CMS.Application.UnitTests.GetAdminSettings;

public class GetAdminSettingsQueryHandlerTests
{
    private readonly GetAdminSettingsQueryHandler _handler;

    public GetAdminSettingsQueryHandlerTests()
    {
        _handler = new GetAdminSettingsQueryHandler();
    }

    [Fact]
    public async Task Handle_ShouldReturnAdminSettings()
    {
        // Act
        var result = await _handler.Handle(new GetAdminSettingsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ServiceFee.Should().Be(50000);
        result.Value.Currency.Should().Be("VND");
        result.Value.MaintenanceMode.Should().BeFalse();
    }
}