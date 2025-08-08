using MediatR;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Features.Carts.DTOs;

namespace Ecommerce.Application.Features.Carts.Queries.Handlers
{
    public class GetCartByUserIdQueryHandler : IRequestHandler<GetCartByUserIdQuery, CartDto>
    {
        private readonly ICartRepository _cartRepository;

        public GetCartByUserIdQueryHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<CartDto> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
            if (cart == null)
                return null;

            var cartDto = new CartDto
            {
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    UnitPrice = ci.UnitPrice,
                    Quantity = ci.Quantity
                }).ToList()
            };

            return cartDto;
        }
    }
}