using MediatR;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Carts.Commands.Handlers
{
    public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, bool>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public AddCartItemCommandHandler(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                return false;

            // Verifica se há estoque suficiente
            if (product.StockQuantity < request.Quantity)
                return false;

            var cart = await _cartRepository.GetByCustomerIdAsync(request.CustomerId);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    CustomerId = request.CustomerId
                };
                cart.AddItem(product, request.Quantity);
                await _cartRepository.AddAsync(cart);
            }
            else
            {
                cart.AddItem(product, request.Quantity);
                await _cartRepository.UpdateAsync(cart);
            }

            return true;
        }
    }
}