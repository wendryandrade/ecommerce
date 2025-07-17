using Ecommerce.Application.Features.Categories.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Categories.Queries
{
    public class GetAllCategoriesQuery : IRequest<List<CategoryDto>>
    {
    }
}
