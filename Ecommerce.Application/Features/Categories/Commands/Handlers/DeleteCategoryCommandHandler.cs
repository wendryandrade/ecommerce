using Ecommerce.Application.Interfaces;
using MediatR;

namespace Ecommerce.Application.Features.Categories.Commands.Handlers
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly ICategoryRepository _repository;

        public DeleteCategoryCommandHandler(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.GetByIdAsync(request.Id);
            if (category == null)
                return false;

            if (category.Products != null && category.Products.Any())
                throw new InvalidOperationException("Não é possível excluir uma categoria com produtos vinculados.");

            await _repository.DeleteAsync(category);
            return true;
        }
    }
}
