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
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;
        private readonly IMemoryCache _memoryCache;
        private const string AllProductsCacheKey = "AllProducts";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            if (_memoryCache.TryGetValue(AllProductsCacheKey, out IEnumerable<ProductDTO> cachedProducts))
            {
                _logger.LogInformation("Returning products from cache");
                return cachedProducts;
            }

            _logger.LogInformation("Fetching products from database");
            var products = await _unitOfWork.Products.GetAllAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDTO>>(products);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(CacheDuration)
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _memoryCache.Set(AllProductsCacheKey, productDtos, cacheEntryOptions);

            return productDtos;
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            string cacheKey = $"Product_{id}";

            if (_memoryCache.TryGetValue(cacheKey, out ProductDTO cachedProduct))
            {
                _logger.LogInformation($"Returning product {id} from cache");
                return cachedProduct;
            }

            _logger.LogInformation($"Fetching product {id} from database");
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            var productDto = _mapper.Map<ProductDTO>(product);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(CacheDuration)
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

            _memoryCache.Set(cacheKey, productDto, cacheEntryOptions);

            return productDto;
        }

        public async Task<ProductDTO> CreateProductAsync(ProductDTO productDto)
        {
            try
            {
                var existingProductId = await _unitOfWork.Products.GetByIdAsync(productDto.ProductId);
                if (existingProductId != null)
                {
                    throw new DuplicateProductException($"A product with the productId '{productDto.ProductId}' already exists.");
                }

                var existingProductName = await _unitOfWork.Products.GetByNameAsync(productDto.Name);
                if (existingProductName != null)
                {
                    throw new DuplicateProductException($"A product with the name '{productDto.Name}' already exists.");
                }

                var product = _mapper.Map<Product>(productDto);
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveAsync();

                var createdProductDto = _mapper.Map<ProductDTO>(product);

                // Invalidate the cache for all products
                _memoryCache.Remove(AllProductsCacheKey);

                // Add the new product to the cache
                _memoryCache.Set($"Product_{product.ProductId}", createdProductDto, CacheDuration);

                _logger.LogInformation($"Product created successfully. Product ID: {product.ProductId}");
                return createdProductDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product");
                throw;
            }
        }

        public async Task UpdateProductAsync(ProductDTO productDto)
        {
            try
            {
                var product = _mapper.Map<Product>(productDto);
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveAsync();

                // Invalidate the cache for all products and the specific product
                _memoryCache.Remove(AllProductsCacheKey);
                _memoryCache.Remove($"Product_{product.ProductId}");

                _logger.LogInformation($"Product updated successfully. Product ID: {product.ProductId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating product with ID {productDto.ProductId}");
                throw;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            try
            {
                await _unitOfWork.Products.DeleteAsync(id);
                await _unitOfWork.SaveAsync();

                // Invalidate the cache for all products and the specific product
                _memoryCache.Remove(AllProductsCacheKey);
                _memoryCache.Remove($"Product_{id}");

                _logger.LogInformation($"Product deleted successfully. Product ID: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting product with ID {id}");
                throw;
            }
        }
    }
}