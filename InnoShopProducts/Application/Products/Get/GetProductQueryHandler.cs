using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Application.Products.Get
{
    public sealed class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductResponse>
    {
        private readonly IProductRepository _productRepository;
        public GetProductQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<ProductResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
            {
                throw new ProductNotFoundException(request.Id);
            }
            var result = new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.Quantity, product.UserId, product.CreatedAt);
            return result;
        }
    }
}
