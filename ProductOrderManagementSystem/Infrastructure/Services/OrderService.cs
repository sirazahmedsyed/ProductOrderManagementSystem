using AutoMapper;
using ProductOrderManagementSystem.Infrastructure.DTOs;
using ProductOrderManagementSystem.Infrastructure.Entities;
using ProductOrderManagementSystem.Infrastructure.Repositories;


namespace ProductOrderManagementSystem.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        //public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        //{
        //    try
        //    {
        //        var orders = await _unitOfWork.Orders.GetAllAsync();
        //        return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while fetching all orders");
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetAllWithIncludesAsync(
                    o => o.Customer,
                    o => o.OrderDetails);
                return _mapper.Map<IEnumerable<OrderDTO>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all orders");
                throw;
            }
        }

        //public async Task<IEnumerable<OrderDTO>> GetAllOrdersWithDetailsAsync()
        //{
        //    try
        //    {
        //        var orders = await _unitOfWork.Orders.GetAllWithIncludesAsync(
        //            o => o.Customer,
        //            o => o.OrderDetails);
        //        return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while fetching all orders with details");
        //        throw;
        //    }
        //}

        public async Task<OrderDTO> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new NotFoundException($"Order with ID {orderId} not found.");
                }
                return _mapper.Map<OrderDTO>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching order with ID {orderId}");
                throw;
            }
        }

        public async Task<OrderDTO> CreateOrderAsync(OrderDTO orderDto)
        {
            try
            {
                var order = _mapper.Map<Order>(orderDto);
                order.OrderDate = DateTime.UtcNow;

                // Fetch or create the customer
                var customer = await _unitOfWork.Customers.GetByIdAsync(orderDto.CustomerId);
                if (customer == null)
                {
                    customer = new Customer { CustomerId = orderDto.CustomerId, Name = orderDto.CustomerName };
                    await _unitOfWork.Customers.AddAsync(customer);
                }
                order.Customer = customer;

                // Group order details by product to prevent duplicates
                var groupedOrderDetails = order.OrderDetails
                    .GroupBy(od => od.ProductId)
                    .Select(g => new OrderDetail
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(od => od.Quantity)
                    })
                    .ToList();

                order.OrderDetails = new List<OrderDetail>();

                foreach (var orderDetail in groupedOrderDetails)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(orderDetail.ProductId);
                    if (product == null)
                    {
                        throw new NotFoundException($"Product with ID {orderDetail.ProductId} not found.");
                    }

                    orderDetail.Product = product;
                    order.OrderDetails.Add(orderDetail);
                }

                // Calculate TotalAmount and DiscountedTotal
                //order.TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.Product.Price * (1 + od.Product.TaxPercentage / 100));
                //order.DiscountedTotal = order.TotalAmount * (1 - order.DiscountPercentage / 100);

                order.CalculateTotals();

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"Order created successfully. Order ID: {order.OrderId}");
                return _mapper.Map<OrderDTO>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order");
                throw;
            }
        }

        //public async Task<OrderDTO> CreateOrderAsync(OrderDTO orderDto)
        //{
        //    try
        //    {
        //        var order = _mapper.Map<Order>(orderDto);
        //         order.OrderDate = DateTime.UtcNow;

        //        var groupedOrderDetails = order.OrderDetails
        //            .GroupBy(od => od.ProductId)
        //            .Select(g => new OrderDetail
        //            {
        //                ProductId = g.Key,
        //                Quantity = g.Sum(od => od.Quantity)
        //            })
        //            .ToList();
        //        order.OrderDetails = groupedOrderDetails;

        //        foreach (var orderDetail in order.OrderDetails)
        //        {
        //            var product = await _unitOfWork.Products.GetByIdAsync(orderDetail.ProductId);
        //            if (product == null)
        //            {
        //                throw new NotFoundException($"Product with ID {orderDetail.ProductId} not found.");
        //            }
        //        }

        //        await _unitOfWork.Orders.AddAsync(order);
        //        await _unitOfWork.SaveAsync();

        //        _logger.LogInformation($"Order created successfully. Order ID: {order.OrderId}");
        //        return _mapper.Map<OrderDTO>(order);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while creating order");
        //        throw;
        //    }
        //}

        public async Task UpdateOrderAsync(OrderDTO orderDto)
        {
            try
            {
                var existingOrder = await _unitOfWork.Orders.GetByIdAsync(orderDto.OrderId);
                if (existingOrder == null)
                {
                    throw new NotFoundException($"Order with ID {orderDto.OrderId} not found.");
                }

                _mapper.Map(orderDto, existingOrder);

                // Re-group order details
                var groupedOrderDetails = existingOrder.OrderDetails
                    .GroupBy(od => od.ProductId)
                    .Select(g => new OrderDetail
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(od => od.Quantity)
                    })
                    .ToList();
                existingOrder.OrderDetails = groupedOrderDetails;

                await _unitOfWork.Orders.UpdateAsync(existingOrder);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"Order updated successfully. Order ID: {existingOrder.OrderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating order with ID {orderDto.OrderId}");
                throw;
            }
        }

        public async Task DeleteOrderAsync(Guid orderId)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
                if (order == null)
                {
                    throw new NotFoundException($"Order with ID {orderId} not found.");
                }

                await _unitOfWork.Orders.DeleteAsync(orderId);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"Order deleted successfully. Order ID: {orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting order with ID {orderId}");
                throw;
            }
        }
        
    }
}
