using Microsoft.EntityFrameworkCore;
using Bookify.Domain.Entities.AccessControl;
using Bookify.Infrastructure.Data;
using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Repositories;

public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly ApplicationDbContext _db;

    public UserRoleRepository(ApplicationDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<Role>> GetActiveRolesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserRole>> GetAllRoleAssignmentsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Include(ur => ur.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId && ur.RoleId == roleId)
            .Include(ur => ur.Role)
            .Include(ur => ur.User)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive, cancellationToken);
    }

    public async Task<bool> UserHasRoleByNameAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return false;

        return await _db.UserRoles
            .AsNoTracking()
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId && ur.IsActive && ur.Role.IsActive && ur.Role.Name == roleName, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetUsersWithRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleId && ur.IsActive)
            .Include(ur => ur.User)
            .Select(ur => ur.User)
            .ToListAsync(cancellationToken);
    }

    public void AddUserRole(UserRole userRole)
    {
        _db.UserRoles.Add(userRole);
    }

    public void UpdateUserRole(UserRole userRole)
    {
        _db.UserRoles.Update(userRole);
    }

    public void RemoveUserRole(UserRole userRole)
    {
        _db.UserRoles.Remove(userRole);
    }

    public async Task<int> GetUserCountForRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _db.UserRoles
            .AsNoTracking()
            .CountAsync(ur => ur.RoleId == roleId && ur.IsActive, cancellationToken);
    }
}
