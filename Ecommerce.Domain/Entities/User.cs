namespace Ecommerce.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Admin" ou "Customer"
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        
        public void AddAddress(Address address) => Addresses.Add(address);
    }
}
