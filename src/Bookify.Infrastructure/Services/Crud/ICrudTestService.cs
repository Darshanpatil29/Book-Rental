using Bookify.Domain.Common;
using Bookify.Domain.Entities;
using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Crud;

public interface ICrudTestService
{
    Task<List<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : DataEntity;
    Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : DataEntity;
    Task<T> CreateAsync<T>(object request, CancellationToken cancellationToken = default) where T : DataEntity;
    Task<T?> UpdateAsync<T>(Guid id, object request, CancellationToken cancellationToken = default) where T : DataEntity;
    Task<bool> DeleteAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : DataEntity;

    Task<List<UserRole>> GetAllUserRolesAsync(CancellationToken cancellationToken = default);
    Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<UserRole> CreateUserRoleAsync(UserRoleRequest request, CancellationToken cancellationToken = default);
    Task<UserRole?> UpdateUserRoleAsync(Guid userId, Guid roleId, UserRoleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);

    Task<List<RolePermission>> GetAllRolePermissionsAsync(CancellationToken cancellationToken = default);
    Task<RolePermission?> GetRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task<RolePermission> CreateRolePermissionAsync(RolePermissionRequest request, CancellationToken cancellationToken = default);
    Task<RolePermission?> UpdateRolePermissionAsync(Guid roleId, Guid permissionId, RolePermissionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRolePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
}