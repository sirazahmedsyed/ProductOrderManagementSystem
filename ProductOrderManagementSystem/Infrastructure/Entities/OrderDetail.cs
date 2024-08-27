using System.ComponentModel.DataAnnotations;

namespace ProductOrderManagementSystem.Infrastructure.Entities
{
    public class OrderDetail
    {
        public Guid OrderDetailId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
