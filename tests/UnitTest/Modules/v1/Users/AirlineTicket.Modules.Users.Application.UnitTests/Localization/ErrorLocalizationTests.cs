using System.Collections.Generic;
using AirlineTicket.BuildingBlocks.Application.Localization;
using AirlineTicket.BuildingBlocks.Infrastructure.Localization;
using AirlineTicket.BuildingBlocks.Responses;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AirlineTicket.Modules.Users.Application.UnitTests.Localization;

public class ErrorLocalizationTests
{
    private readonly HeaderLanguageResolver _languageResolver;
    private readonly JsonErrorLocalizer _localizer;

    public ErrorLocalizationTests()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _languageResolver = new HeaderLanguageResolver(httpContextAccessorMock.Object);
        _localizer = new JsonErrorLocalizer(_languageResolver);
    }

    [Theory]
    [InlineData("vi", null, "vi")]
    [InlineData("VI-vn", null, "vi")]
    [InlineData("zh-CN", null, "zh")]
    [InlineData("ja-JP", null, "ja")]
    [InlineData("ko_KR", null, "ko")]
    [InlineData("fr-FR", null, "fr")]
    [InlineData("de-DE", null, "en")] // Unsupported -> fallback default
    public void ResolveLanguage_From_XLanguageHeader_ShouldResolveCorrectly(string xLanguage, string? acceptLanguage, string expected)
    {
        var resolved = _languageResolver.ResolveLanguageFromHeaders(xLanguage, acceptLanguage);
        Assert.Equal(expected, resolved);
    }

    [Fact]
    public void ResolveLanguage_AcceptLanguage_QualityWeight_ShouldPickHighestSupported()
    {
        // fr-CH has highest q, followed by ja, then en
        var acceptHeader = "fr-CH;q=0.9, ja-JP;q=0.8, en-US;q=0.5";
        var resolved = _languageResolver.ResolveLanguageFromHeaders(null, acceptHeader);
        Assert.Equal("fr", resolved);

        // ja has higher q than vi
        var acceptHeader2 = "vi-VN;q=0.3, ja-JP;q=0.7";
        var resolved2 = _languageResolver.ResolveLanguageFromHeaders(null, acceptHeader2);
        Assert.Equal("ja", resolved2);
    }

    [Fact]
    public void ResolveLanguage_MalformedAcceptLanguage_ShouldFallbackGracefully()
    {
        var malformed = "invalid;;q=abc, ;;, ,";
        var resolved = _languageResolver.ResolveLanguageFromHeaders(null, malformed);
        Assert.Equal("en", resolved);
    }

    [Fact]
    public void Localize_KnownErrorCode_ShouldReturnTranslatedString()
    {
        var viMessage = _localizer.Localize("User.NotFound", culture: "vi");
        Assert.Equal("Không tìm thấy thông tin người dùng.", viMessage);

        var jaMessage = _localizer.Localize("User.NotFound", culture: "ja");
        Assert.Equal("ユーザーが見つかりません。", jaMessage);

        var enMessage = _localizer.Localize("User.NotFound", culture: "en");
        Assert.Equal("User not found.", enMessage);
    }

    [Fact]
    public void Localize_WithDynamicArgs_ShouldInterpolateParameters()
    {
        var localized = _localizer.Localize("Seat.Conflict", culture: "vi", args: new object[] { "12A" });
        Assert.Equal("Ghế 12A đã được đặt hoặc không còn khả dụng.", localized);

        var localizedEn = _localizer.Localize("Seat.Conflict", culture: "en", args: new object[] { "12A" });
        Assert.Equal("Seat 12A is already reserved or unavailable.", localizedEn);
    }

    [Fact]
    public void Localize_UntranslatedOrMissingKey_ShouldFallbackToEnglishOrRawCode()
    {
        // Non-existent key with fallback message
        var msg = _localizer.Localize("CUSTOM_UNKNOWN_CODE", fallbackMessage: "Default fallback message", culture: "ja");
        Assert.Equal("Default fallback message", msg);

        // Non-existent key without fallback message
        var msgRaw = _localizer.Localize("CUSTOM_UNKNOWN_CODE", fallbackMessage: null, culture: "fr");
        Assert.Equal("CUSTOM_UNKNOWN_CODE", msgRaw);
    }

    [Fact]
    public void GetLocalizedError_ShouldPreserveCodeAndLocalizeMessage()
    {
        var error = Error.Create("Seat.Conflict", "Seat conflict fallback", "14C");
        var localizedError = _localizer.GetLocalizedError(error, culture: "vi");

        Assert.Equal("Seat.Conflict", localizedError.Code);
        Assert.Equal("Ghế 14C đã được đặt hoặc không còn khả dụng.", localizedError.Message);
    }
}
