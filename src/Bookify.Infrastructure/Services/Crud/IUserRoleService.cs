using Bookify.Domain.Entities.AccessControl;

namespace Bookify.Infrastructure.Services.Crud;

public interface IUserRoleService
{
    Task<List<UserRole>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserRole?> GetByIdAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<UserRole> CreateAsync(UserRoleRequest request, CancellationToken cancellationToken = default);
    Task<UserRole?> UpdateAsync(Guid userId, Guid roleId, UserRoleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}