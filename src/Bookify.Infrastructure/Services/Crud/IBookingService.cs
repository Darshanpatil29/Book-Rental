using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Services.Crud;

public interface IBookingService
{
    Task<List<Booking>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Booking> CreateAsync(BookingRequest request, CancellationToken cancellationToken = default);
    Task<Booking?> UpdateAsync(Guid id, BookingRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}