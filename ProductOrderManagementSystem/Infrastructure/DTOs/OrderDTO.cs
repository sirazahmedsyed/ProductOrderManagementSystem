namespace ProductOrderManagementSystem.Infrastructure.DTOs
{
    public class OrderDTO
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal DiscountPercentage { get; set; }
        public ICollection<OrderDetailDTO>? OrderDetails { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountedTotal { get; set; }
    }

}
