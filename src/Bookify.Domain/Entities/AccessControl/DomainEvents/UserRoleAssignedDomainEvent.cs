using Bookify.Domain.Common;

namespace Bookify.Domain.Entities.AccessControl.DomainEvents;

/// <summary>
/// Domain event raised when a role is assigned to a user.
/// </summary>
public record UserRoleAssignedDomainEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    DateTime AssignedOnUtc) : DomainEvent;
