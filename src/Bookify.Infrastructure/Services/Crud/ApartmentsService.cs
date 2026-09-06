using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Services.Crud;

public sealed class ApartmentsService : IApartmentService
{
    private readonly ICrudTestService _crudService;

    public ApartmentsService(ICrudTestService crudService)
    {
        _crudService = crudService;
    }

    public Task<List<Apartment>> GetAllAsync(CancellationToken cancellationToken = default)
        => _crudService.GetAllAsync<Apartment>(cancellationToken);

    public Task<Apartment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.GetByIdAsync<Apartment>(id, cancellationToken);

    public Task<Apartment> CreateAsync(ApartmentRequest request, CancellationToken cancellationToken = default)
        => _crudService.CreateAsync<Apartment>(request, cancellationToken);

    public Task<Apartment?> UpdateAsync(Guid id, ApartmentRequest request, CancellationToken cancellationToken = default)
        => _crudService.UpdateAsync<Apartment>(id, request, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.DeleteAsync<Apartment>(id, cancellationToken);
}