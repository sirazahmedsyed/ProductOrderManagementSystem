namespace ProductOrderManagementSystem.Infrastructure.DTOs
{
    public class OrderDTO
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public ICollection<OrderDetailDTO> OrderDetails { get; set; } = new HashSet<OrderDetailDTO>();
        public decimal TotalAmount { get; set; }
    }

}
