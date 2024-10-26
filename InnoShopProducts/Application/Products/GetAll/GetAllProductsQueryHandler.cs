using Domain.Entities;
using MediatR;

namespace Application.Products.GetAll
{
    internal sealed class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        public GetAllProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<List<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetAllAsync();
            if (products == null || products.Count == 0)
                throw new NoProductsException();
            return products.Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Quantity,
                p.UserId,
                p.CreatedAt
            )).ToList();
        }
    }
}
