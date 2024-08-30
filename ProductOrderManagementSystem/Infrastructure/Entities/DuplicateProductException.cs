namespace ProductOrderManagementSystem.Infrastructure.Entities
{
    public class DuplicateProductException : Exception
    {
        public DuplicateProductException(string message) : base(message) { }
    }
}
