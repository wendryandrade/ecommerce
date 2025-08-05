namespace Ecommerce.API.Resources.Users.DTOs.Responses
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}