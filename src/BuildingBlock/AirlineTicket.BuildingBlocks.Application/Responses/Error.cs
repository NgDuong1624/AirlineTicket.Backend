using System.Text.Json.Serialization;

namespace AirlineTicket.BuildingBlocks.Responses;

public class Error
{
    public string Code { get; }
    public string Message { get; }

    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public object[]? Args { get; }

    [Newtonsoft.Json.JsonConstructor]
    [JsonConstructor]
    public Error(string code, string message, object[]? args = null)
    {
        Code = code ?? string.Empty;
        Message = message ?? string.Empty;
        Args = args;
    }

    public static Error Create(string code, string message, params object[] args) => new(code, message, args);

    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
}
