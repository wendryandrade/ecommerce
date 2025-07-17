using Ecommerce.Application.Features.Categories.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Categories.Queries
{
    public class GetCategoryByIdQuery : IRequest<CategoryDto?>
    {
        public Guid Id { get; set; }
    }
}
