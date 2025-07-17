using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Carts.DTOs
{
    public class CartDto
    {
        public Guid CustomerId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
