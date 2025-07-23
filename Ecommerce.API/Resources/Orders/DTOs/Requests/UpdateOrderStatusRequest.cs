using Ecommerce.Domain.Enums;

namespace Ecommerce.API.Resources.Orders.DTOs.Requests
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus NewStatus { get; set; }
    }
}