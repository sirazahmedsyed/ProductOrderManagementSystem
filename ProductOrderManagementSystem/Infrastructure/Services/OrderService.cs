using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ProductOrderManagementSystem.Infrastructure.DTOs;
using ProductOrderManagementSystem.Infrastructure.Entities;
using ProductOrderManagementSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductOrderManagementSystem.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        private readonly IMemoryCache _memoryCache;
        private const string AllOrdersCacheKey = "AllOrders";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrderService> logger, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            if (_memoryCache.TryGetValue(AllOrdersCacheKey, out IEnumerable<OrderDTO> cachedOrders))
            {
                _logger.LogInformation("Returning orders from cache");
                return cachedOrders;
            }

            _logger.LogInformation("Fetching orders from database");
            var orders = await _unitOfWork.Orders.GetAllWithIncludesAsync(
                o => o.Customer,
                o => o.OrderDetails);
            var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(CacheDuration)
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _memoryCache.Set(AllOrdersCacheKey, orderDtos, cacheEntryOptions);

            return orderDtos;
        }

        public async Task<OrderDTO> GetOrderByIdAsync(Guid orderId)
        {
            string cacheKey = $"Order_{orderId}";

            if (_memoryCache.TryGetValue(cacheKey, out OrderDTO cachedOrder))
            {
                _logger.LogInformation($"Returning order {orderId} from cache");
                return cachedOrder;
            }

            _logger.LogInformation($"Fetching order {orderId} from database");
            var order = await _unitOfWork.Orders.GetByIdWithIncludesAsync(
                orderId,
                o => o.Customer,
                o => o.OrderDetails);

            if (order == null)
            {
                throw new NotFoundException($"Order with ID {orderId} not found.");
            }

            var orderDto = _mapper.Map<OrderDTO>(order);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(CacheDuration)
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

            _memoryCache.Set(cacheKey, orderDto, cacheEntryOptions);

            return orderDto;
        }

        

        public async Task<OrderDTO> CreateOrderAsync(OrderDTO orderDto)
        {
            try
            {
                var order = _mapper.Map<Order>(orderDto);
                order.OrderDate = DateTime.UtcNow;

                var customer = await _unitOfWork.Customers.GetByNameAsync(orderDto.CustomerName);
                if (customer == null)
                {
                    customer = new Customer { CustomerId = orderDto.CustomerId, Name = orderDto.CustomerName };
                    await _unitOfWork.Customers.AddAsync(customer);
                }
                order.Customer = customer;

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

                await CalculateOrderTotals(order);

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveAsync();

                var createdOrderDto = _mapper.Map<OrderDTO>(order);

                // Invalidate the cache for all orders
                _memoryCache.Remove(AllOrdersCacheKey);

                // Add the new order to the cache
                _memoryCache.Set($"Order_{order.OrderId}", createdOrderDto, CacheDuration);

                _logger.LogInformation($"Order created successfully. Order ID: {order.OrderId}");
                return createdOrderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order");
                throw;
            }
        }

        

        public async Task<OrderDTO> UpdateOrderAsync(OrderDTO orderDto)
        {
            try
            {
                var existingOrder = await _unitOfWork.Orders.GetByIdWithIncludesAsync(
                    orderDto.OrderId,
                    o => o.Customer,
                    o => o.OrderDetails);

                if (existingOrder == null)
                {
                    throw new NotFoundException($"Order with ID {orderDto.OrderId} not found.");
                }

                // Update Customer
                if (existingOrder.Customer.CustomerId != orderDto.CustomerId)
                {
                    var newCustomer = await _unitOfWork.Customers.GetByIdAsync(orderDto.CustomerId);
                    if (newCustomer == null)
                    {
                        newCustomer = new Customer { CustomerId = orderDto.CustomerId, Name = orderDto.CustomerName };
                        await _unitOfWork.Customers.AddAsync(newCustomer);
                    }
                    existingOrder.Customer = newCustomer;
                }
                else
                {
                    // Update customer name if it has changed
                    if (existingOrder.Customer.Name != orderDto.CustomerName)
                    {
                        existingOrder.Customer.Name = orderDto.CustomerName;
                    }
                }

                // Update other order properties
                existingOrder.DiscountPercentage = orderDto.DiscountPercentage;

                // Update OrderDetails
                existingOrder.OrderDetails.Clear();
                foreach (var detailDto in orderDto.OrderDetails)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(detailDto.ProductId);
                    if (product == null)
                    {
                        throw new NotFoundException($"Product with ID {detailDto.ProductId} not found.");
                    }

                    var orderDetail = new OrderDetail
                    {
                        ProductId = detailDto.ProductId,
                        Quantity = detailDto.Quantity,
                        Product = product
                    };
                    existingOrder.OrderDetails.Add(orderDetail);
                }

                await CalculateOrderTotals(existingOrder);

                await _unitOfWork.Orders.UpdateAsync(existingOrder);
                await _unitOfWork.SaveAsync();

                // Invalidate the cache for all orders and the specific order
                _memoryCache.Remove(AllOrdersCacheKey);
                _memoryCache.Remove($"Order_{existingOrder.OrderId}");

                _logger.LogInformation($"Order updated successfully. Order ID: {existingOrder.OrderId}");

                // Map the updated order back to DTO and return it
                return _mapper.Map<OrderDTO>(existingOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating order with ID {orderDto.OrderId}");
                throw;
            }
        }
        private async Task CalculateOrderTotals(Order order)
        {
            order.TotalAmount = 0;
            foreach (var orderDetail in order.OrderDetails)
            {
                if (orderDetail.Product == null)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(orderDetail.ProductId);
                    if (product == null)
                    {
                        throw new NotFoundException($"Product with ID {orderDetail.ProductId} not found.");
                    }
                    orderDetail.Product = product;
                }

                order.TotalAmount += orderDetail.Quantity * orderDetail.Product.Price * (1 + orderDetail.Product.TaxPercentage / 100);
            }

            order.DiscountedTotal = order.TotalAmount * (1 - order.DiscountPercentage / 100);
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

                // Invalidate the cache for all orders and the specific order
                _memoryCache.Remove(AllOrdersCacheKey);
                _memoryCache.Remove($"Order_{orderId}");

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