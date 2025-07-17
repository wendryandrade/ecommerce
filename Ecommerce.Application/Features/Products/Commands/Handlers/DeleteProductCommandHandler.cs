using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Commands;
using MediatR;

namespace Ecommerce.Application.Features.Products.Commands.Handlers
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductRepository _repository;

        public DeleteProductCommandHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null)
                return false;

            await _repository.DeleteAsync(product);
            return true;
        }
    }
}
