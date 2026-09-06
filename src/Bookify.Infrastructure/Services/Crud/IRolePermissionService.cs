using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Crud;

public interface IRolePermissionService
{
    Task<List<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RolePermission?> GetByIdAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task<RolePermission> CreateAsync(RolePermissionRequest request, CancellationToken cancellationToken = default);
    Task<RolePermission?> UpdateAsync(Guid roleId, Guid permissionId, RolePermissionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
}