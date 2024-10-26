using MediatR;
using Domain.Entities;
using Application.Data;

namespace Application.Products.Delete
{
    internal class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private IUnitOfWork _unitOfWork;
        public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
            {
                throw new ProductNotFoundException(request.Id);
            }

            if (product.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("User is not allowed to delete this product.");
            }
            _productRepository.Remove(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
