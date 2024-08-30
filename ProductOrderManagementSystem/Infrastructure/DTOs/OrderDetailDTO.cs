namespace ProductOrderManagementSystem.Infrastructure.DTOs
{

    public class OrderDetailDTO
    {
        public Guid OrderDetailId { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
       
    }
}
