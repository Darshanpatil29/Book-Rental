using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Crud;

public sealed class RolePermissionsService : IRolePermissionService
{
    private readonly ICrudTestService _crudService;

    public RolePermissionsService(ICrudTestService crudService)
    {
        _crudService = crudService;
    }

    public Task<List<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default)
        => _crudService.GetAllRolePermissionsAsync(cancellationToken);

    public Task<RolePermission?> GetByIdAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        => _crudService.GetRolePermissionAsync(roleId, permissionId, cancellationToken);

    public Task<RolePermission> CreateAsync(RolePermissionRequest request, CancellationToken cancellationToken = default)
        => _crudService.CreateRolePermissionAsync(request, cancellationToken);

    public Task<RolePermission?> UpdateAsync(Guid roleId, Guid permissionId, RolePermissionRequest request, CancellationToken cancellationToken = default)
        => _crudService.UpdateRolePermissionAsync(roleId, permissionId, request, cancellationToken);

    public Task<bool> DeleteAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        => _crudService.DeleteRolePermissionAsync(roleId, permissionId, cancellationToken);
}