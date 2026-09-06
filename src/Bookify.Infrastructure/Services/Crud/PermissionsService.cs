using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Crud;

public sealed class PermissionsService : IPermissionService
{
    private readonly ICrudTestService _crudService;

    public PermissionsService(ICrudTestService crudService)
    {
        _crudService = crudService;
    }

    public Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
        => _crudService.GetAllAsync<Permission>(cancellationToken);

    public Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.GetByIdAsync<Permission>(id, cancellationToken);

    public Task<Permission> CreateAsync(PermissionRequest request, CancellationToken cancellationToken = default)
        => _crudService.CreateAsync<Permission>(request, cancellationToken);

    public Task<Permission?> UpdateAsync(Guid id, PermissionRequest request, CancellationToken cancellationToken = default)
        => _crudService.UpdateAsync<Permission>(id, request, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.DeleteAsync<Permission>(id, cancellationToken);
}