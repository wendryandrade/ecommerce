using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Carts.Commands
{
    public class RemoveCartItemCommand : IRequest<bool>
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }

        public RemoveCartItemCommand(Guid customerId, Guid productId)
        {
            CustomerId = customerId;
            ProductId = productId;
        }
    }
}
