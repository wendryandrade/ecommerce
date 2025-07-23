using Ecommerce.Domain.Enums;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Commands
{
    public class UpdateOrderStatusCommand : IRequest<bool>
    {
        public Guid OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
    }
}