using System.Text.RegularExpressions;

namespace AirlineTicket.BuildingBlocks.Caching;

/// <summary>
/// Standardized cache key builder.
/// Ensures consistent key format across modules for future Redis cluster / microservice cache sharing.
/// </summary>
public static partial class CacheKeyBuilder
{
    // Pattern: {Module}:{Entity}:{Identifier}
    // Examples: Flights:Search:origin=HAN_dest=SGN_date=20260625
    //           Bookings:Ticket:guid

    [GeneratedRegex(@"[^a-zA-Z0-9_:=-]")]
    private static partial Regex InvalidKeyChars();

    /// <summary>
    /// Build a cache key from segments.
    /// </summary>
    public static string Build(string module, string entity, string identifier)
    {
        var joined = $"{module}:{entity}:{identifier}";
        return InvalidKeyChars().Replace(joined, "_");
    }

    /// <summary>
    /// Build a cache key from a type and a unique identifier.
    /// </summary>
    public static string ForQuery<T>(string identifier)
    {
        var typeName = typeof(T).Name;
        // Strip common suffixes for cleaner keys
        var cleanName = typeName
            .Replace("Query", "")
            .Replace("Command", "");
        return Build("App", cleanName, identifier);
    }
}
