using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ProductOrderManagementSystem.Infrastructure.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(object id);
        Task<T> GetByNameAsync(string name);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(object id);

        Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includeProperties);
        Task<T> GetByIdWithIncludesAsync(object id, params Expression<Func<T, object>>[] includeProperties);
    }
}
