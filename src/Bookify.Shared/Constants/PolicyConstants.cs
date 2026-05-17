namespace Bookify.Shared.Constants;

/// <summary>
/// Contains all permission keys and policy names for Role-Based Access Control (RBAC).
/// Format: "Feature.Action" for easy organization and querying.
/// </summary>
public static class PolicyConstants
{
    // ============================================================================
    // BOOKINGS PERMISSIONS
    // ============================================================================

    public static class Bookings
    {
        public const string ViewAll = "Bookings.ViewAll";
        public const string ViewOwn = "Bookings.ViewOwn";
        public const string Create = "Bookings.Create";
        public const string Confirm = "Bookings.Confirm";
        public const string Reject = "Bookings.Reject";
        public const string Cancel = "Bookings.Cancel";
        public const string Complete = "Bookings.Complete";
    }

    // ============================================================================
    // APARTMENTS PERMISSIONS
    // ============================================================================

    public static class Apartments
    {
        public const string ViewAll = "Apartments.ViewAll";
        public const string ViewOwn = "Apartments.ViewOwn";
        public const string Create = "Apartments.Create";
        public const string Update = "Apartments.Update";
        public const string UpdateOwn = "Apartments.UpdateOwn";
        public const string Delete = "Apartments.Delete";
        public const string DeleteOwn = "Apartments.DeleteOwn";
        public const string Activate = "Apartments.Activate";
        public const string Deactivate = "Apartments.Deactivate";
    }

    // ============================================================================
    // REVIEWS PERMISSIONS
    // ============================================================================

    public static class Reviews
    {
        public const string ViewAll = "Reviews.ViewAll";
        public const string Create = "Reviews.Create";
        public const string Update = "Reviews.Update";
        public const string Delete = "Reviews.Delete";
        public const string Approve = "Reviews.Approve";
        public const string Disapprove = "Reviews.Disapprove";
    }

    // ============================================================================
    // USERS PERMISSIONS
    // ============================================================================

    public static class Users
    {
        public const string ViewAll = "Users.ViewAll";
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string UpdateOwn = "Users.UpdateOwn";
        public const string Delete = "Users.Delete";
        public const string ManageRoles = "Users.ManageRoles";
        public const string ManagePermissions = "Users.ManagePermissions";
        public const string Activate = "Users.Activate";
        public const string Deactivate = "Users.Deactivate";
    }

    // ============================================================================
    // ROLES & PERMISSIONS MANAGEMENT
    // ============================================================================

    public static class RoleManagement
    {
        public const string ViewAll = "RoleManagement.ViewAll";
        public const string Create = "RoleManagement.Create";
        public const string Update = "RoleManagement.Update";
        public const string Delete = "RoleManagement.Delete";
        public const string ManagePermissions = "RoleManagement.ManagePermissions";
    }

    // ============================================================================
    // REPORTS & ANALYTICS PERMISSIONS
    // ============================================================================

    public static class Reports
    {
        public const string ViewBookingAnalytics = "Reports.ViewBookingAnalytics";
        public const string ViewApartmentAnalytics = "Reports.ViewApartmentAnalytics";
        public const string ViewRevenueReports = "Reports.ViewRevenueReports";
        public const string ViewUserAnalytics = "Reports.ViewUserAnalytics";
        public const string ExportReports = "Reports.ExportReports";
    }

    // ============================================================================
    // SYSTEM ADMINISTRATION
    // ============================================================================

    public static class SystemAdmin
    {
        public const string ViewLogs = "SystemAdmin.ViewLogs";
        public const string ManageSystemSettings = "SystemAdmin.ManageSystemSettings";
        public const string ManageDatabase = "SystemAdmin.ManageDatabase";
        public const string ViewAuditTrail = "SystemAdmin.ViewAuditTrail";
    }

    // ============================================================================
    // POLICY NAMES (for Authorization attributes)
    // ============================================================================

    public static class Policies
    {
        // Guest policies (least privileged)
        public const string GuestOnly = "GuestOnly";
        public const string GuestOrOwner = "GuestOrOwner";

        // Owner policies
        public const string OwnerOnly = "OwnerOnly";

        // Admin policies
        public const string AdminOnly = "AdminOnly";

        // Authenticated policies (any authenticated user)
        public const string Authenticated = "Authenticated";

        // Feature-specific policies
        public const string CanViewApartments = "CanViewApartments";
        public const string CanCreateBooking = "CanCreateBooking";
        public const string CanManageOwnBookings = "CanManageOwnBookings";
        public const string CanManageApartments = "CanManageApartments";
        public const string CanWriteReviews = "CanWriteReviews";
    }

    // ============================================================================
    // ROLE NAMES (standard roles)
    // ============================================================================

    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Owner = "Owner";
        public const string Guest = "Guest";
    }
}
