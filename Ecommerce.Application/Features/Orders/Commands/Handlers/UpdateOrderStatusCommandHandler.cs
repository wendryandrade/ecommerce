using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Interfaces;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Handlers
{
    public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
    {
        private readonly IOrderRepository _orderRepository;

        public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

            if (order == null)
            {
                return false;
            }

            order.Status = request.NewStatus;

            await _orderRepository.UpdateAsync(order);

            return true;
        }
    }
}