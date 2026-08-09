namespace AirlineTicket.BuildingBlocks.Domain.Constants;

/// <summary>
/// Centralized authorization constants, claim types, and policy prefixes.
/// Shared across all modules.
/// </summary>
public static class AuthConstants
{
    public static class Claims
    {
        public const string Subject = "sub";
        public const string Email = "email";
        public const string Role = "Role";
        public const string FullName = "FullName";
        public const string AirlineId = "AirlineId";
        public const string Permission = "Permission";
        public const string AirportScope = "AirportScope";
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Partner = "Partner";
        public const string Staff = "Staff";
        public const string Customer = "Customer";
    }

    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string PartnerOnly = "PartnerOnly";
        public const string StaffOnly = "StaffOnly";
        public const string PartnerOrStaff = "PartnerOrStaff";

        // Dynamic policy prefix
        public const string PermissionPrefix = "Permission:";
        public const string AirlineBound = "AirlineBound";
    }
}
