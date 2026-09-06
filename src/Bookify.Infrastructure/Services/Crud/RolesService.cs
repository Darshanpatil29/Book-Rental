using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Crud;

public sealed class RolesService : IRoleService
{
    private readonly ICrudTestService _crudService;

    public RolesService(ICrudTestService crudService)
    {
        _crudService = crudService;
    }

    public Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        => _crudService.GetAllAsync<Role>(cancellationToken);

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.GetByIdAsync<Role>(id, cancellationToken);

    public Task<Role> CreateAsync(RoleRequest request, CancellationToken cancellationToken = default)
        => _crudService.CreateAsync<Role>(request, cancellationToken);

    public Task<Role?> UpdateAsync(Guid id, RoleRequest request, CancellationToken cancellationToken = default)
        => _crudService.UpdateAsync<Role>(id, request, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.DeleteAsync<Role>(id, cancellationToken);
}