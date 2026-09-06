using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Crud;

public sealed class UserRolesService : IUserRoleService
{
    private readonly ICrudTestService _crudService;

    public UserRolesService(ICrudTestService crudService)
    {
        _crudService = crudService;
    }

    public Task<List<UserRole>> GetAllAsync(CancellationToken cancellationToken = default)
        => _crudService.GetAllUserRolesAsync(cancellationToken);

    public Task<UserRole?> GetByIdAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        => _crudService.GetUserRoleAsync(userId, roleId, cancellationToken);

    public Task<UserRole> CreateAsync(UserRoleRequest request, CancellationToken cancellationToken = default)
        => _crudService.CreateUserRoleAsync(request, cancellationToken);

    public Task<UserRole?> UpdateAsync(Guid userId, Guid roleId, UserRoleRequest request, CancellationToken cancellationToken = default)
        => _crudService.UpdateUserRoleAsync(userId, roleId, request, cancellationToken);

    public Task<bool> DeleteAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        => _crudService.DeleteUserRoleAsync(userId, roleId, cancellationToken);
}