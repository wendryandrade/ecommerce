using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Infrastructure;
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
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService; 

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IShippingService shippingService,
            IPaymentService paymentService) 
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _shippingService = shippingService;
            _paymentService = paymentService; 
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("O carrinho está vazio.");
            }

            var orderItems = new List<OrderItem>();
            decimal itemsTotalAmount = 0;

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

                itemsTotalAmount += cartItem.Quantity * cartItem.UnitPrice;
            }

            // Calcular o frete
            var originZipCode = "32000000"; // CEP de origem 
            var shippingCost = await _shippingService.CalculateShippingCostAsync(originZipCode, request.ShippingAddress.PostalCode);

            var finalTotalAmount = itemsTotalAmount + shippingCost;

            // Processar o pagamento ANTES de salvar o pedido
            var testPaymentMethodId = "pm_card_visa";
            var transactionId = await _paymentService.ProcessPaymentAsync(finalTotalAmount, "brl", testPaymentMethodId);

            // Criar o Pedido com os dados corretos
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = finalTotalAmount,
                Status = OrderStatus.Paid, // O pedido já nasce como "Pago"
                OrderItems = orderItems,
                ShippingAddress = new Address
                {
                    Id = Guid.NewGuid(),
                    Street = request.ShippingAddress.Street,
                    City = request.ShippingAddress.City,
                    State = request.ShippingAddress.State,
                    PostalCode = request.ShippingAddress.PostalCode
                },
                Payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    Amount = finalTotalAmount, // O valor do pagamento é o valor final
                    PaymentMethod = request.PaymentDetails.PaymentMethod,
                    Status = PaymentStatus.Paid, // O pagamento já foi bem-sucedido
                    TransactionId = transactionId // O ID real retornado pelo Stripe
                }
            };

            await _orderRepository.AddAsync(order, cancellationToken);

            cart.Clear();
            await _cartRepository.UpdateAsync(cart);

            return order.Id;
        }
    }
}