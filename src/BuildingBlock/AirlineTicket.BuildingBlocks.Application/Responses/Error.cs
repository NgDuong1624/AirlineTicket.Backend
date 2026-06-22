using System.Text.Json.Serialization;

namespace AirlineTicket.BuildingBlocks.Responses;

public class Error
{
    public string Code { get; }
    public string Message { get; }

    [Newtonsoft.Json.JsonConstructor]
    [JsonConstructor]
    public Error(string code, string message)
    {
        Code = code ?? string.Empty;
        Message = message ?? string.Empty;
    }

    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
}
