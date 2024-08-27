using ProductOrderManagementSystem.Infrastructure.DTOs;

namespace ProductOrderManagementSystem.Infrastructure.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO> GetProductByIdAsync(int id);
        Task<ProductDTO> CreateProductAsync(ProductDTO productDto);
        Task UpdateProductAsync(ProductDTO productDto);
        Task DeleteProductAsync(int id);
    }
}
