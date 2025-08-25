using Ecommerce.Application.Features.Orders.Events;
using Ecommerce.Application.Interfaces;
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

            _logger.LogInformation("Calculando frete para CEP de destino: {DestinationCep}", message.ShippingAddress.PostalCode);
            var (shippingCost, deliveryDays, _, _, _) = await _shippingService.CalculateShippingWithDetailsFromStoreAsync(message.ShippingAddress.PostalCode);
            _logger.LogInformation("Frete calculado: R$ {ShippingCost}", shippingCost);
            
            var finalTotalAmount = itemsTotalAmount + shippingCost;
            _logger.LogInformation("Valor total do pedido: R$ {TotalAmount} (itens: R$ {ItemsAmount} + frete: R$ {ShippingCost})", finalTotalAmount, itemsTotalAmount, shippingCost);
            
            _logger.LogInformation("Processando pagamento via Stripe: R$ {Amount}, moeda: brl", finalTotalAmount);
            var transactionId = await _paymentService.ProcessPaymentAsync(finalTotalAmount, "brl", "pm_card_visa");
            _logger.LogInformation("Pagamento processado com sucesso! Transaction ID: {TransactionId}", transactionId);

            var now = DateTime.UtcNow;
            var order = new Order
            {
                Id = message.OrderId,
                UserId = message.UserId,
                OrderDate = now,
                TotalAmount = finalTotalAmount,
                Status = OrderStatus.Paid,
                OrderItems = orderItems,
                ShippingAddress = new Address
                {
                    Id = Guid.NewGuid(),
                    UserId = message.UserId,
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
                },
                Shipping = new Shipping
                {
                    Id = Guid.NewGuid(),
                    ShippingCost = shippingCost,
                    EstimatedDeliveryDate = now.Date.AddDays(deliveryDays)
                }
            };

            await _orderRepository.AddAsync(order, context.CancellationToken);
            _logger.LogInformation("Pedido {OrderId} processado e salvo com sucesso!", message.OrderId);
        }
    }
}