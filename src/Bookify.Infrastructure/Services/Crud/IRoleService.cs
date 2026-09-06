using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Crud;

public interface IRoleService
{
    Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role> CreateAsync(RoleRequest request, CancellationToken cancellationToken = default);
    Task<Role?> UpdateAsync(Guid id, RoleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}