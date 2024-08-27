namespace ProductOrderManagementSystem.Infrastructure.DTOs
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal TaxPercentage { get; set; }
    }
}
