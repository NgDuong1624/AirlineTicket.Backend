using System.Net.Http.Headers;
using AirlineTicket.BuildingBlocks.Domain.Constants;

namespace AirlineTicket.IntegrationTests.Shared.Auth;

public static class HttpClientAuthExtensions
{
    public static HttpClient AsAuthenticatedUser(
        this HttpClient client,
        Guid? userId = null,
        string role = AuthConstants.Roles.Customer,
        string email = "testuser@airlineticket.com",
        string fullName = "Test User",
        Guid? airlineId = null,
        IEnumerable<string>? permissions = null)
    {
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.EmailHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.FullNameHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.AirlineIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.PermissionsHeader);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthOptions.Scheme, "fake-token");
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, (userId ?? Guid.NewGuid()).ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, email);
        client.DefaultRequestHeaders.Add(TestAuthHandler.FullNameHeader, fullName);

        if (airlineId.HasValue)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.AirlineIdHeader, airlineId.Value.ToString());
        }

        if (permissions != null)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, string.Join(",", permissions));
        }

        return client;
    }

    public static HttpClient AsAdmin(this HttpClient client, Guid? adminId = null)
    {
        return client.AsAuthenticatedUser(
            userId: adminId ?? Guid.NewGuid(),
            role: AuthConstants.Roles.Admin,
            email: "admin@airlineticket.com",
            fullName: "System Admin");
    }

    public static HttpClient AsCustomer(this HttpClient client, Guid? customerId = null)
    {
        return client.AsAuthenticatedUser(
            userId: customerId ?? Guid.NewGuid(),
            role: AuthConstants.Roles.Customer,
            email: "customer@airlineticket.com",
            fullName: "Customer User");
    }

    public static HttpClient AsPartner(this HttpClient client, Guid airlineId, Guid? partnerId = null)
    {
        return client.AsAuthenticatedUser(
            userId: partnerId ?? Guid.NewGuid(),
            role: AuthConstants.Roles.Partner,
            email: "partner@airline.com",
            fullName: "Partner Representative",
            airlineId: airlineId);
    }

    public static HttpClient AsStaff(this HttpClient client, Guid airlineId, Guid? staffId = null)
    {
        return client.AsAuthenticatedUser(
            userId: staffId ?? Guid.NewGuid(),
            role: AuthConstants.Roles.Staff,
            email: "staff@airline.com",
            fullName: "Staff Member",
            airlineId: airlineId);
    }

    public static HttpClient AsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.EmailHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.FullNameHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.AirlineIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.PermissionsHeader);
        return client;
    }
}
