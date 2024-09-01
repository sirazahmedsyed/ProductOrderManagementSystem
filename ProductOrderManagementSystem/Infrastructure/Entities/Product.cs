using System.ComponentModel.DataAnnotations;

namespace ProductOrderManagementSystem.Infrastructure.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public decimal TaxPercentage { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
