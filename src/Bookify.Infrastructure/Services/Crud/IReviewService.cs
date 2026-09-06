using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Services.Crud;

public interface IReviewService
{
    Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Review> CreateAsync(ReviewRequest request, CancellationToken cancellationToken = default);
    Task<Review?> UpdateAsync(Guid id, ReviewRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}