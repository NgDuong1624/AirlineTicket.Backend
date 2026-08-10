using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Application.Features.Admin;
using AirlineTicket.BuildingBlocks.Caching;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Application.UnitTests.CreatePromotion;

public class CreatePromotionCommandHandlerTests
{
    private readonly Mock<IPromotionRepository> _promoRepoMock;

    public CreatePromotionCommandHandlerTests()
    {
        _promoRepoMock = new Mock<IPromotionRepository>();
    }

    [Fact]
    public async Task CreatePromotionCommandHandler_ShouldCreatePromotion()
    {
        var handler = new CreatePromotionCommandHandler(_promoRepoMock.Object, Mock.Of<ICacheService>());
        var command = new CreatePromotionCommand("Summer Sale", "SUMMER", "Percentage", 10, 100, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

        var expectedId = Guid.NewGuid();
        _promoRepoMock.Setup(x => x.CreateAsync(It.IsAny<PromotionDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedId);
        _promoRepoMock.Verify(x => x.CreateAsync(It.IsAny<PromotionDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}