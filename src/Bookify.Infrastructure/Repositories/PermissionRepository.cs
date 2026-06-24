using Microsoft.EntityFrameworkCore;
using Bookify.Domain.Entities.AccessControl;
using Bookify.Infrastructure.Data;

namespace Bookify.Infrastructure.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _db;

    public PermissionRepository(ApplicationDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<Permission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await _db.Permissions
            .AsNoTracking()
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.Id == permissionId, cancellationToken);
    }

    public async Task<Permission?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        return await _db.Permissions
            .AsNoTracking()
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.Key == key, cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Permissions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Permissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByFeatureAsync(string feature, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(feature))
            return Array.Empty<Permission>();

        return await _db.Permissions
            .AsNoTracking()
            .Where(p => p.Feature == feature)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Permissions assigned to roles that the user currently has (active assignments)
        var roleIds = await _db.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Select(ur => ur.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!roleIds.Any())
            return Array.Empty<Permission>();

        return await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public void Add(Permission permission)
    {
        _db.Permissions.Add(permission);
    }

    public void Update(Permission permission)
    {
        _db.Permissions.Update(permission);
    }

    public void Remove(Permission permission)
    {
        _db.Permissions.Remove(permission);
    }

    public async Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return await _db.Permissions
            .AsNoTracking()
            .AnyAsync(p => p.Key == key, cancellationToken);
    }

    public void AddRange(IEnumerable<Permission> permissions)
    {
        _db.Permissions.AddRange(permissions);
    }
}
