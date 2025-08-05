using Ecommerce.Application.Interfaces;
using MediatR;

namespace Ecommerce.Application.Features.Carts.Commands.Handlers
{
    public class DecreaseCartItemQuantityCommandHandler : IRequestHandler<DecreaseCartItemQuantityCommand, bool>
    {
        private readonly ICartRepository _cartRepository;

        public DecreaseCartItemQuantityCommandHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<bool> Handle(DecreaseCartItemQuantityCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
            if (cart == null) return false;

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
            if (item == null) return false;

            item.Quantity -= 1;

            if (item.Quantity <= 0)
                cart.RemoveItem(request.ProductId);

            await _cartRepository.UpdateAsync(cart);
            return true;
        }
    }
}
