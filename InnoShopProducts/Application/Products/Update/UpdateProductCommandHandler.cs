using MediatR;
using Application.Data;
using Domain.Entities;

namespace Application.Products.Update
{
    internal sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private IUnitOfWork _unitOfWork;
        public UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
            {
                throw new ProductNotFoundException(request.Id);
            }
            if (product.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("User is not allowed to update this product.");
            }
            product.Update(request.Name, request.Description, request.Price, request.Quantity);
            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
