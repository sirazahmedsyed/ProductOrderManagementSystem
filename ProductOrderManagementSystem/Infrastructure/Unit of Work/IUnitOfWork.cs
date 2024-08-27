using ProductOrderManagementSystem.Infrastructure.Entities;
using ProductOrderManagementSystem.Infrastructure.Repositories;

using System.Threading.Tasks;

namespace ProductOrderManagementSystem.Infrastructure.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderDetail> OrderDetails { get; }
        Task<int> SaveAsync();
    }
}


