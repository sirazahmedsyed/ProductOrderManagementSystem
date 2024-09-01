using ProductOrderManagementSystem.Infrastructure.DTOs;

namespace ProductOrderManagementSystem.Infrastructure.Services
{
    
    public interface IOrderService
    {
    Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
    Task<OrderDTO> GetOrderByIdAsync(Guid orderId);
    Task<OrderDTO> CreateOrderAsync(OrderDTO orderDTO);

    Task<OrderDTO> UpdateOrderAsync(OrderDTO orderDTO);
    Task DeleteOrderAsync(Guid orderId);

   
    }
}
