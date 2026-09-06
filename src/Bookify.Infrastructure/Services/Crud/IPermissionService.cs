using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Crud;

public interface IPermissionService
{
    Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Permission> CreateAsync(PermissionRequest request, CancellationToken cancellationToken = default);
    Task<Permission?> UpdateAsync(Guid id, PermissionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}