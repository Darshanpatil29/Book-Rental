using Bookify.Domain.Common;

namespace Bookify.Domain.Entities.AccessControl.DomainEvents;

/// <summary>
/// Domain event raised when a role is removed from a user.
/// </summary>
public record UserRoleRemovedDomainEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    DateTime RemovedOnUtc) : DomainEvent;
