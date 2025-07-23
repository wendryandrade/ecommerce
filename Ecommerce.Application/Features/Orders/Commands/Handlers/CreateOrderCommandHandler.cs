using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Handlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByCustomerIdAsync(request.CustomerId);
            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("O carrinho está vazio.");
            }

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var cartItem in cart.CartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null || product.StockQuantity < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Produto '{cartItem.Product?.Name ?? "ID: " + cartItem.ProductId}' fora de estoque.");
                }

                product.DecreaseStock(cartItem.Quantity);
                await _productRepository.UpdateAsync(product);

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice
                });

                totalAmount += cartItem.Quantity * cartItem.UnitPrice;
            }

            // MUDANÇA CRÍTICA AQUI: Criamos o Pedido com suas dependências aninhadas
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                OrderItems = orderItems,

                // O Endereço de Entrega é criado DIRETAMENTE AQUI
                ShippingAddress = new Address
                {
                    Id = Guid.NewGuid(),
                    Street = request.ShippingAddress.Street,
                    City = request.ShippingAddress.City,
                    State = request.ShippingAddress.State,
                    PostalCode = request.ShippingAddress.PostalCode
                },

                // O Pagamento é criado DIRETAMENTE AQUI
                Payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    Amount = totalAmount,
                    PaymentMethod = request.PaymentDetails.PaymentMethod,
                    Status = PaymentStatus.Pending,
                    TransactionId = "trans_" + Guid.NewGuid().ToString().Substring(0, 8)
                }
            };

            // Agora, ao adicionar o pedido, o EF Core entende que ele "possui"
            // um Endereço e um Pagamento, e salva todos juntos.
            await _orderRepository.AddAsync(order, cancellationToken);

            cart.Clear();
            await _cartRepository.UpdateAsync(cart);

            return order.Id;
        }
    }
}