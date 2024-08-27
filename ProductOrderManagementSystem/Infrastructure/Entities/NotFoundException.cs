namespace ProductOrderManagementSystem.Infrastructure.Entities
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
