using System.Threading.Tasks;
using ProductOrderManagementSystem.Infrastructure.DBContext;
using ProductOrderManagementSystem.Infrastructure.Entities;
using ProductOrderManagementSystem.Infrastructure.Repositories;

namespace ProductOrderManagementSystem.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IGenericRepository<Product> _products;
        private IGenericRepository<Order> _orders;
        private IGenericRepository<OrderDetail> _orderDetails;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Product> Products => _products ??= new GenericRepository<Product>(_context);
        public IGenericRepository<Order> Orders => _orders ??= new GenericRepository<Order>(_context);
        public IGenericRepository<OrderDetail> OrderDetails => _orderDetails ??= new GenericRepository<OrderDetail>(_context);

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

