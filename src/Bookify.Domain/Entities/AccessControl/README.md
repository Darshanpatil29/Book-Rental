# Role-Based Access Control (RBAC) Implementation

## Overview

This directory contains a comprehensive Role-Based Access Control (RBAC) system for the Bookify platform. It implements a flexible permission model where permissions are assigned to roles, and roles are assigned to users.

## Architecture

```
User
  ↓
UserRole (junction/many-to-many)
  ↓
Role
  ↓
RolePermission (junction/many-to-many)
  ↓
Permission
```

## Components

### 1. **Domain Entities**

#### `Role.cs`
- Represents a role in the system (Admin, Owner, Guest)
- Contains permissions collection
- Methods:
  - `Create()` - Factory method for creating roles
  - `AddPermission()` - Add permission to role
  - `RemovePermission()` - Remove permission from role
  - `HasPermission()` - Check if role has permission
  - `UpdateInfo()` - Update role details
  - `Activate()` / `Deactivate()` - Toggle role availability

#### `Permission.cs`
- Represents a granular permission (e.g., "Bookings.Create", "Apartments.Update")
- Format: "Feature.Action"
- Properties:
  - `Key` - Unique identifier (e.g., "Bookings.Create")
  - `Feature` - Feature/module name (e.g., "Bookings")
  - `Action` - Action name (e.g., "Create")
  - `Name` - Display name
  - `Description` - What the permission allows
  - `IsActive` - Can be assigned to roles
  - `SortOrder` - Display order in UI

#### `UserRole.cs` (Junction Entity)
- Many-to-many mapping between User and Role
- Properties:
  - `UserId` / `RoleId` - Composite key
  - `AssignedOnUtc` - When role was assigned
  - `RemovedOnUtc` - When role was removed (soft delete)
  - `IsActive` - Whether assignment is currently active
- Methods:
  - `Create()` - Factory method
  - `Remove()` / `Restore()` - Soft delete operations

#### `RolePermission.cs` (Junction Entity)
- Many-to-many mapping between Role and Permission
- Composite key: `(RoleId, PermissionId)`
- Tracks when permission was assigned to role

### 2. **User Entity Extensions**

Updated `User.cs` with:
- `UserRoles` collection - All assigned roles (read-only)
- `AssignRole()` - Assign a role to user
- `RemoveRole()` - Remove a role from user
- `HasRole()` - Check if user has specific role
- `GetActiveRoles()` - Get all active roles for user

### 3. **Repositories**

#### `IRoleRepository.cs`
- Manages role persistence
- Methods:
  - `GetByIdAsync()` / `GetByNameAsync()`
  - `GetAllActiveAsync()` / `GetAllAsync()`
  - `GetRolesByUserIdAsync()`
  - `ExistsByNameAsync()`
  - CRUD: `Add()`, `Update()`, `Remove()`

#### `IPermissionRepository.cs`
- Manages permission persistence
- Methods:
  - `GetByIdAsync()` / `GetByKeyAsync()`
  - `GetAllActiveAsync()` / `GetAllAsync()`
  - `GetByFeatureAsync()` - Get all permissions for a feature
  - `GetByRoleIdAsync()` - Get permissions assigned to role
  - `GetByUserIdAsync()` - Get all user's permissions (via roles)
  - `ExistsByKeyAsync()`
  - `AddRange()` - Bulk add for seeding
  - CRUD: `Add()`, `Update()`, `Remove()`

#### `IUserRoleRepository.cs`
- Manages user-role assignments
- Methods:
  - `GetActiveRolesForUserAsync()` - Get active roles
  - `UserHasRoleAsync()` / `UserHasRoleByNameAsync()`
  - `GetUsersWithRoleAsync()` - Reverse lookup
  - `GetUserCountForRoleAsync()`
  - CRUD: `AddUserRole()`, `UpdateUserRole()`, `RemoveUserRole()`

### 4. **Services**

#### `IRbacService.cs` (Domain Service Interface)
- High-level RBAC operations
- Methods:
  - `HasPermissionAsync()` - Check single permission
  - `HasAnyPermissionAsync()` - Check multiple (OR logic)
  - `HasAllPermissionsAsync()` - Check multiple (AND logic)
  - `GetUserPermissionsAsync()` - Get all user permissions
  - `GetUserRolesAsync()` - Get all user roles
  - `AssignRoleToUserAsync()` / `RemoveRoleFromUserAsync()`
  - `UserHasRoleAsync()` - Check by role name

### 5. **Constants**

#### `PolicyConstants.cs` (in Shared/Constants)
Centralized permission and policy names:

```csharp
// Feature permissions
PolicyConstants.Bookings.Create
PolicyConstants.Bookings.Confirm
PolicyConstants.Apartments.Update
PolicyConstants.Users.ManageRoles

// Policy names (for Authorization attributes)
PolicyConstants.Policies.AdminOnly
PolicyConstants.Policies.OwnerOnly
PolicyConstants.Policies.GuestOrOwner

// Standard role names
PolicyConstants.RoleNames.Admin
PolicyConstants.RoleNames.Owner
PolicyConstants.RoleNames.Guest
```

### 6. **Value Objects**

#### `PermissionClaim.cs` (in Shared/ValueObjects)
- Immutable representation of a permission for caching
- Properties: PermissionKey, Feature, Action, Name, IsActive
- Methods: `Matches()` for checking claims

### 7. **Domain Events**

#### `UserRoleAssignedDomainEvent.cs`
- Raised when a role is assigned to a user
- Data: UserId, RoleId, RoleName, AssignedOnUtc

#### `UserRoleRemovedDomainEvent.cs`
- Raised when a role is removed from a user
- Data: UserId, RoleId, RoleName, RemovedOnUtc

## Permission Structure

Permissions follow a hierarchical naming convention:

```
Feature.Action

Examples:
Bookings.ViewAll
Bookings.ViewOwn
Bookings.Create
Bookings.Confirm
Bookings.Cancel
Apartments.Create
Apartments.Update
Apartments.UpdateOwn
Apartments.Delete
Users.ManageRoles
Reports.ViewAnalytics
SystemAdmin.ViewLogs
```

## Typical Usage Flow

### 1. Create Permissions (Seed)
```csharp
var createBookingPermission = Permission.Create(
    "Bookings",
    "Create",
    "Create Booking",
    "Allows user to create a new booking",
    sortOrder: 1
);

var confirmBookingPermission = Permission.Create(
    "Bookings",
    "Confirm",
    "Confirm Booking",
    "Allows user to confirm a pending booking",
    sortOrder: 2
);

await permissionRepository.AddRange(new[] { 
    createBookingPermission.Value,
    confirmBookingPermission.Value 
});
```

### 2. Create Roles
```csharp
var guestRole = Role.Create(
    "Guest",
    "Basic user who can make bookings and leave reviews"
);

var ownerRole = Role.Create(
    "Owner",
    "Property owner who manages apartments and bookings"
);
```

### 3. Assign Permissions to Roles
```csharp
guestRole.Value.AddPermission(createBookingPermission.Value);
guestRole.Value.AddPermission(confirmBookingPermission.Value);

ownerRole.Value.AddPermission(createApartmentPermission.Value);
ownerRole.Value.AddPermission(updateApartmentPermission.Value);
```

### 4. Assign Roles to Users
```csharp
var userRole = UserRole.Create(userId, guestRoleId);
user.AssignRole(userRole);

// Or via RBAC service
await rbacService.AssignRoleToUserAsync(userId, guestRoleId);
```

### 5. Check Permissions
```csharp
// Check single permission
bool canCreateBooking = await rbacService.HasPermissionAsync(
    userId, 
    PolicyConstants.Bookings.Create
);

// Check multiple permissions (OR)
bool canManageBooking = await rbacService.HasAnyPermissionAsync(
    userId,
    new[] { 
        PolicyConstants.Bookings.Confirm,
        PolicyConstants.Bookings.Cancel,
        PolicyConstants.Bookings.Complete
    }
);

// Check all permissions (AND)
bool isFullOwner = await rbacService.HasAllPermissionsAsync(
    userId,
    new[] {
        PolicyConstants.Apartments.Create,
        PolicyConstants.Apartments.Update,
        PolicyConstants.Apartments.Delete
    }
);
```

## Authorization Attributes (Future)

Once implemented in the API layer:

```csharp
[Authorize(Policy = PolicyConstants.Policies.AdminOnly)]
[HttpGet("admin/reports")]
public async Task<IActionResult> GetReports()

[Authorize(Policy = PolicyConstants.Policies.OwnerOnly)]
[HttpPost("apartments")]
public async Task<IActionResult> CreateApartment([FromBody] CreateApartmentRequest request)

[Authorize]
[HttpPost("bookings")]
public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
```

## Database Schema (EF Core)

### Tables
- `roles` - Role entities
- `permissions` - Permission entities
- `role_permissions` - Junction table
- `user_roles` - Junction table

### Indexes
- Unique: (Role.Name)
- Unique: (Permission.Key)
- Unique: (RoleId, PermissionId)
- Unique: (UserId, RoleId) where RemovedOnUtc IS NULL

## Soft Delete Pattern

UserRole uses soft delete:
- `RemovedOnUtc` is set when role is removed
- `IsActive` property checks if RemovedOnUtc is null
- Allows audit trail and role restoration

## Best Practices

1. **Permission Naming** - Always use "Feature.Action" format
2. **Role Granularity** - Create specific roles for common scenarios
3. **Permission Caching** - Cache user permissions with TTL to reduce DB queries
4. **Audit Trail** - Use domain events to track role changes
5. **Least Privilege** - Assign minimum required permissions
6. **Regular Review** - Audit role-permission assignments periodically

## Implementation Checklist

- [ ] Create permissions via seeding/migration
- [ ] Create standard roles (Admin, Owner, Guest)
- [ ] Assign permissions to roles
- [ ] Implement RbacService in Infrastructure
- [ ] Implement repositories in Infrastructure
- [ ] Add policy configurations in API Program.cs
- [ ] Add authorization attributes to controllers
- [ ] Create event handlers for role change events
- [ ] Add permission caching mechanism
- [ ] Create admin UI for role/permission management

---

**Location**: `src/Bookify.Domain/Entities/AccessControl/`
**Related Shared**: `src/Bookify.Shared/Constants/PolicyConstants.cs`, `src/Bookify.Shared/ValueObjects/PermissionClaim.cs`
