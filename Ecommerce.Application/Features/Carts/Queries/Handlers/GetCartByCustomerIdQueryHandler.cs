using MediatR;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Features.Carts.DTOs;

namespace Ecommerce.Application.Features.Carts.Queries.Handlers
{
    public class GetCartByCustomerIdQueryHandler : IRequestHandler<GetCartByCustomerIdQuery, CartDto>
    {
        private readonly ICartRepository _cartRepository;

        public GetCartByCustomerIdQueryHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<CartDto> Handle(GetCartByCustomerIdQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByCustomerIdAsync(request.CustomerId);
            if (cart == null)
                return null;

            var cartDto = new CartDto
            {
                CustomerId = cart.CustomerId,
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