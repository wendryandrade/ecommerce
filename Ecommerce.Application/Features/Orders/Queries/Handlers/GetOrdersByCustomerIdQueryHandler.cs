using Ecommerce.Application.Features.Addresses.DTOs;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Payments.DTOs;
using Ecommerce.Application.Interfaces;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Queries.Handlers
{
    public class GetOrdersByCustomerIdQueryHandler : IRequestHandler<GetOrdersByCustomerIdQuery, List<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrdersByCustomerIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<OrderDto>> Handle(GetOrdersByCustomerIdQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

            var orderDtos = orders.Select(order => new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderItems = order.OrderItems.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "Produto não encontrado",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList(),    
                Payment = order.Payment == null ? null : new PaymentDto
                {
                    PaymentMethod = order.Payment.PaymentMethod,
                    Status = order.Payment.Status,
                    Amount = order.Payment.Amount,
                    TransactionId = order.Payment.TransactionId
                },
                ShippingAddress = order.ShippingAddress == null ? null : new AddressDto
                {
                    Street = order.ShippingAddress.Street,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State,
                    PostalCode = order.ShippingAddress.PostalCode
                }
            }).ToList();

            return orderDtos;
        }
    }
}