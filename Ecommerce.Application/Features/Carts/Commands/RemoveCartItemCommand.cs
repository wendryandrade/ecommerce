using MediatR;

namespace Ecommerce.Application.Features.Carts.Commands
{
    public class RemoveCartItemCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }

        public RemoveCartItemCommand(Guid userId, Guid productId)
        {
            UserId = userId;
            ProductId = productId;
        }
    }
}
