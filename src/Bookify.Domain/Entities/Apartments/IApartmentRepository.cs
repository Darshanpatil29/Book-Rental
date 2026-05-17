using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookify.Domain.Entities.Apartments
{
    public interface IApartmentRepository
    {
        Task<Apartment> GetByIdAsync(Guid id);
        Task<IEnumerable<Apartment>> GetAllAsync();
        Task AddAsync(Apartment apartment);
        Task UpdateAsync(Apartment apartment);
        Task DeleteAsync(Guid id);
    }
}