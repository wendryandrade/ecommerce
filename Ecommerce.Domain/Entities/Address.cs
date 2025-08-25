namespace Ecommerce.Domain.Entities
{
    public class Address
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}