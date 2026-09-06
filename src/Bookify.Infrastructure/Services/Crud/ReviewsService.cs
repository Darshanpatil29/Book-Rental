using Bookify.Domain.Entities;

namespace Bookify.Infrastructure.Services.Crud;

public sealed class ReviewsService : IReviewService
{
    private readonly ICrudTestService _crudService;

    public ReviewsService(ICrudTestService crudService)
    {
        _crudService = crudService;
    }

    public Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default)
        => _crudService.GetAllAsync<Review>(cancellationToken);

    public Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.GetByIdAsync<Review>(id, cancellationToken);

    public Task<Review> CreateAsync(ReviewRequest request, CancellationToken cancellationToken = default)
        => _crudService.CreateAsync<Review>(request, cancellationToken);

    public Task<Review?> UpdateAsync(Guid id, ReviewRequest request, CancellationToken cancellationToken = default)
        => _crudService.UpdateAsync<Review>(id, request, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _crudService.DeleteAsync<Review>(id, cancellationToken);
}