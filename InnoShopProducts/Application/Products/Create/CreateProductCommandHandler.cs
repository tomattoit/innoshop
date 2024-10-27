using MediatR;
using Domain.Entities;
using Application.Data;

namespace Application.Products.Create
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
    {
        private readonly IProductRepository _productRepository;
        private IUnitOfWork _unitOfWork;
        public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product(Guid.NewGuid(), request.Name, request.Description, request.Price, request.Quantity, request.UserId);
            _productRepository.Add(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
