using Ecommerce.Application.Interfaces;
using MediatR;

namespace Ecommerce.Application.Features.Categories.Commands.Handlers
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
    {
        private readonly ICategoryRepository _repository;

        public UpdateCategoryCommandHandler(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.GetByIdAsync(request.Id);
            if (category == null) return false;

            category.Name = request.Name;
            category.Description = request.Description;
            await _repository.UpdateAsync(category);
            return true;
        }
    }
}
