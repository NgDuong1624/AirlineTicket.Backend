namespace AirlineTicket.LoadTests.Common;

public record LoadTestConfig
{
    public string BaseUrl { get; init; } = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5179";
    public HttpClient HttpClient { get; init; } = new() { Timeout = TimeSpan.FromSeconds(5) };
    public string SampleBookingId { get; init; } = "b1b0f2a8-9b2f-4a9b-89e3-4e80d77bc901";
}
