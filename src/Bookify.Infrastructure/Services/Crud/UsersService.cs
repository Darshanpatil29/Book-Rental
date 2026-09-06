using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Services.Crud;

public sealed class UsersService : IUserService
{
    private readonly ICrudTestService _crudService;

    public UsersService(ICrudTestService crudService)
    {
        _crudService = crudService;
    }

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => _crudService.GetAllAsync<User>(cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.GetByIdAsync<User>(id, cancellationToken);

    public Task<User> CreateAsync(UserRequest request, CancellationToken cancellationToken = default)
        => _crudService.CreateAsync<User>(request, cancellationToken);

    public Task<User?> UpdateAsync(Guid id, UserRequest request, CancellationToken cancellationToken = default)
        => _crudService.UpdateAsync<User>(id, request, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.DeleteAsync<User>(id, cancellationToken);
}