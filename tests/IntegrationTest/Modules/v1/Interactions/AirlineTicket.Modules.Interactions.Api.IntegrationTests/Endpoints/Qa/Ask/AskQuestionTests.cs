using System.Net;
using System.Net.Http.Json;
using AirlineTicket.IntegrationTests.Shared;
using AirlineTicket.IntegrationTests.Shared.Auth;
using AirlineTicket.Modules.Interactions.Api.Endpoints;
using FluentAssertions;
using Xunit;

namespace AirlineTicket.Modules.Interactions.Api.IntegrationTests.Endpoints.Qa.Ask;

[Collection("InteractionsTests")]
public class AskQuestionTests : BaseIntegrationTest
{
    public AskQuestionTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AskQuestion_ShouldReturnBadRequest_WhenQuestionIsEmpty()
    {
        // Arrange
        var request = new AskQuestionRequest(string.Empty);

        // Act
        var response = await Client.AsAnonymous().PostAsJsonAsync("/api/v1/qa/ask", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "POST /api/v1/qa/ask: AskQuestion must return 400 BadRequest but return {0}", response.StatusCode);
    }
}
