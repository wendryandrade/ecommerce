using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.Events;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using MassTransit;
using MediatR;
using System.Linq;

namespace Ecommerce.Application.Features.Orders.Handlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateOrderCommandHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IPublishEndpoint publishEndpoint)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
            if (cart == null || cart.CartItems.Count == 0)
            {
                throw new InvalidOperationException("O carrinho está vazio.");
            }

            // Validação de estoque 
            foreach (var cartItem in cart.CartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null || product.StockQuantity < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Produto '{product?.Name ?? "ID: " + cartItem.ProductId}' fora de estoque.");
                }
            }

            var orderId = Guid.NewGuid();

            // Publicar evento para processamento assíncrono
            var orderEvent = new OrderSubmissionEvent
            {
                OrderId = orderId,
                UserId = request.UserId,
                ShippingAddress = request.ShippingAddress,
                PaymentDetails = request.PaymentDetails,
                CartItems = cart.CartItems.Select(item => new EventCartItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            await _publishEndpoint.Publish(orderEvent, cancellationToken);
            
            // Limpa o carrinho do usuário
            cart.Clear();
            await _cartRepository.UpdateAsync(cart);

            // Retorna o ID do pedido para o cliente, para que ele possa acompanhá-lo
            return orderId;
        }
    }
}