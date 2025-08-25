using Ecommerce.Application.Features.Addresses.DTOs;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Queries;
using Ecommerce.Application.Features.Payments.DTOs;
using Ecommerce.Application.Interfaces;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Queries.Handlers
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);

            if (order == null)
                return null;

            // Mapeamento manual
            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderItems = order.OrderItems.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList(),
                Payment = order.Payment != null ? new PaymentDto
                {
                    PaymentMethod = order.Payment.PaymentMethod,
                    Status = order.Payment.Status,
                    Amount = order.Payment.Amount,
                    TransactionId = order.Payment.TransactionId 
                } : null,
                ShippingAddress = order.ShippingAddress != null ? new AddressDto
                {
                    Street = order.ShippingAddress.Street,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State,
                    PostalCode = order.ShippingAddress.PostalCode
                } : null,
                // Map shipping info (sem carrier/tracking)
                Shipping = order.Shipping != null ? new ShippingDto
                {
                    ShippingCost = order.Shipping.ShippingCost,
                    EstimatedDeliveryDate = order.Shipping.EstimatedDeliveryDate,
                    DeliveryDays = (int)Math.Max(0, (order.Shipping.EstimatedDeliveryDate.Date - order.OrderDate.Date).TotalDays)
                } : null
            };

            return orderDto;
        }
    }
}