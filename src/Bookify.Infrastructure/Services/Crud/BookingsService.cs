using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Services.Crud;

public sealed class BookingsService : IBookingService
{
    private readonly ICrudTestService _crudService;

    public BookingsService(ICrudTestService crudService)
    {
        _crudService = crudService;
    }

    public Task<List<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
        => _crudService.GetAllAsync<Booking>(cancellationToken);

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.GetByIdAsync<Booking>(id, cancellationToken);

    public Task<Booking> CreateAsync(BookingRequest request, CancellationToken cancellationToken = default)
        => _crudService.CreateAsync<Booking>(request, cancellationToken);

    public Task<Booking?> UpdateAsync(Guid id, BookingRequest request, CancellationToken cancellationToken = default)
        => _crudService.UpdateAsync<Booking>(id, request, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.DeleteAsync<Booking>(id, cancellationToken);
}