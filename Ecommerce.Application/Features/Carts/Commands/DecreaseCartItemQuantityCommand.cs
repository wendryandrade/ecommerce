using MediatR;

namespace Ecommerce.Application.Features.Carts.Commands
{
    public class DecreaseCartItemQuantityCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
