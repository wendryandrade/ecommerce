using MediatR;

namespace Ecommerce.Application.Features.Carts.Commands
{
    public class DecreaseCartItemQuantityCommand : IRequest<bool>
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
