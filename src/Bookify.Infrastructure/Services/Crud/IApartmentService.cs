using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Services.Crud;

public interface IApartmentService
{
    Task<List<Apartment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Apartment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Apartment> CreateAsync(ApartmentRequest request, CancellationToken cancellationToken = default);
    Task<Apartment?> UpdateAsync(Guid id, ApartmentRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}