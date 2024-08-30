using ProductOrderManagementSystem.Infrastructure.DTOs;

namespace ProductOrderManagementSystem.Infrastructure.Services
{
    
    public interface IOrderService
    {
    Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
    Task<OrderDTO> GetOrderByIdAsync(Guid orderId);
    Task<OrderDTO> CreateOrderAsync(OrderDTO orderDTO);
    Task UpdateOrderAsync(OrderDTO orderDTO);
    Task DeleteOrderAsync(Guid orderId);

    //Task<IEnumerable<OrderDTO>> GetAllOrdersWithDetailsAsync();

        //Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        //Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        //   Task<OrderDto> GetOrderByIdAsync(Guid orderId);
        //   Task<OrderDto> CreateOrderAsync(OrderDto orderDto);
        //    Task UpdateOrderAsync(OrderDto orderDto);
        //    Task DeleteOrderAsync(Guid orderId);
    }
}
