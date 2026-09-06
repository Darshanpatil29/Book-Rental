using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Services.Crud;

public interface IUserService
{
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(UserRequest request, CancellationToken cancellationToken = default);
    Task<User?> UpdateAsync(Guid id, UserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}