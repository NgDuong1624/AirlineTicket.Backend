using AirlineTicket.Modules.Promotions.Application.Contracts;
using AirlineTicket.Modules.Promotions.Application.Features.Admin;
using AirlineTicket.Modules.Promotions.Application.Features.Public;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AirlineTicket.Modules.Promotions.Application.UnitTests;

public class PromotionHandlersTests
{
    private readonly Mock<IPromotionRepository> _promoRepoMock;

    public PromotionHandlersTests()
    {
        _promoRepoMock = new Mock<IPromotionRepository>();
    }

    [Fact]
    public async Task ApplyPromotionCommandHandler_ShouldCalculateDiscount_WhenValid()
    {
        var handler = new ApplyPromotionCommandHandler(_promoRepoMock.Object);
        var command = new ApplyPromotionCommand("SUMMER", Guid.NewGuid(), 1000m);

        var promo = new PromotionDto
        {
            PromoCode = "SUMMER",
            DiscountType = "Percentage",
            DiscountValue = 10, // 10%
            EndDate = DateTime.UtcNow.AddDays(1),
            MaxUsage = 100,
            CurrentUsage = 0
        };

        _promoRepoMock.Setup(x => x.GetByCodeAsync("SUMMER", It.IsAny<CancellationToken>()))
            .ReturnsAsync(promo);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.DiscountAmount.Should().Be(100m);
        result.Value.FinalAmount.Should().Be(900m);
    }

    [Fact]
    public async Task CreatePromotionCommandHandler_ShouldCreatePromotion()
    {
        var handler = new CreatePromotionCommandHandler(_promoRepoMock.Object);
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
