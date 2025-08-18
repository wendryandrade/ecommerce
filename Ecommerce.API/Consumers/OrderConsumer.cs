using Ecommerce.Application.Features.Orders.Events;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Infrastructure;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using MassTransit;

namespace Ecommerce.API.Consumers
{
    public class OrderConsumer : IConsumer<OrderSubmissionEvent>
    {
        private readonly ILogger<OrderConsumer> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;

        // O Consumer recebe todas as dependências que o CommandHandler usava
        public OrderConsumer(
            ILogger<OrderConsumer> logger,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IShippingService shippingService,
            IPaymentService paymentService)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _shippingService = shippingService;
            _paymentService = paymentService;
        }

        // Este é o método que o MassTransit chama quando uma mensagem chega na fila
        public async Task Consume(ConsumeContext<OrderSubmissionEvent> context)
        {
            _logger.LogInformation("Recebido evento para processar pedido {OrderId}", context.Message.OrderId);

            var message = context.Message;

            // --- A LÓGICA QUE ANTES ESTAVA NO COMMANDHANDLER AGORA VIVE AQUI ---

            var orderItems = new List<OrderItem>();
            decimal itemsTotalAmount = 0;

            foreach (var item in message.CartItems)
            {
                // Re-verifica o estoque aqui, pois ele pode ter mudado
                // entre o momento em que o cliente clicou e o processamento
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    _logger.LogError("Produto {ProductId} fora de estoque para o pedido {OrderId}. Evento será descartado.", item.ProductId, message.OrderId);
                    // Em um cenário real, você publicaria um evento "PedidoFalhou" aqui
                    // para notificar o usuário. Por enquanto, apenas paramos o processamento.
                    return;
                }

                product.DecreaseStock(item.Quantity);
                await _productRepository.UpdateAsync(product);

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
                itemsTotalAmount += item.Quantity * item.UnitPrice;
            }

            var shippingCost = await _shippingService.CalculateShippingCostAsync("32000000", message.ShippingAddress.PostalCode);
            var finalTotalAmount = itemsTotalAmount + shippingCost;
            var transactionId = await _paymentService.ProcessPaymentAsync(finalTotalAmount, "brl", "pm_card_visa");

            var order = new Order
            {
                Id = message.OrderId,
                UserId = message.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = finalTotalAmount,
                Status = OrderStatus.Paid,
                OrderItems = orderItems,
                ShippingAddress = new Address
                {
                    Id = Guid.NewGuid(),
                    Street = message.ShippingAddress.Street,
                    City = message.ShippingAddress.City,
                    State = message.ShippingAddress.State,
                    PostalCode = message.ShippingAddress.PostalCode
                },
                Payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    Amount = finalTotalAmount,
                    PaymentMethod = message.PaymentDetails.PaymentMethod,
                    Status = PaymentStatus.Paid,
                    TransactionId = transactionId
                }
            };

            await _orderRepository.AddAsync(order, context.CancellationToken);
            _logger.LogInformation("Pedido {OrderId} processado e salvo com sucesso!", message.OrderId);
        }
    }
}