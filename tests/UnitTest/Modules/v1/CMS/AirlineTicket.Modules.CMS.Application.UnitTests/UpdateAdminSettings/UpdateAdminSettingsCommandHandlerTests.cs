using AirlineTicket.Modules.CMS.Application.Features.Settings;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.CMS.Application.UnitTests.UpdateAdminSettings;

public class UpdateAdminSettingsCommandHandlerTests
{
    private readonly UpdateAdminSettingsCommandHandler _handler;

    public UpdateAdminSettingsCommandHandlerTests()
    {
        _handler = new UpdateAdminSettingsCommandHandler();
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess()
    {
        // Act
        var result = await _handler.Handle(new UpdateAdminSettingsCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}