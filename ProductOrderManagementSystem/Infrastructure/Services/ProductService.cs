using AutoMapper;
using ProductOrderManagementSystem.Infrastructure.DTOs;
using ProductOrderManagementSystem.Infrastructure.Entities;
using ProductOrderManagementSystem.Infrastructure.Repositories;

namespace ProductOrderManagementSystem.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            try
            {
                var products = await _unitOfWork.Products.GetAllAsync();
                return _mapper.Map<IEnumerable<ProductDTO>>(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all products");
                throw;
            }
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    throw new NotFoundException($"Product with ID {id} not found.");
                }
                return _mapper.Map<ProductDTO>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching product with ID {id}");
                throw;
            }
        }

        //public async Task<ProductDTO> CreateProductAsync(ProductDTO productDto)
        //{
        //    try
        //    {
        //        var product = _mapper.Map<Product>(productDto);
        //        await _unitOfWork.Products.AddAsync(product);
        //        await _unitOfWork.SaveAsync();
        //        _logger.LogInformation($"Product created successfully. Product ID: {product.ProductId}");
        //        return _mapper.Map<ProductDTO>(product);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while creating product");
        //        throw;
        //    }
        //}
        public async Task<ProductDTO> CreateProductAsync(ProductDTO productDto)
        {
            try
            {
                // Check if a product with the same name already exists
                var existingProduct = await _unitOfWork.Products.GetByNameAsync(productDto.Name);
                if (existingProduct != null)
                {
                    throw new DuplicateProductException($"A product with the name '{productDto.Name}' already exists.");
                }

                var product = _mapper.Map<Product>(productDto);
                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation($"Product created successfully. Product ID: {product.ProductId}");
                return _mapper.Map<ProductDTO>(product);
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
