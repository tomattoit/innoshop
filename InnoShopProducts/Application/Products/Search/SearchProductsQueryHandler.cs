using MediatR;
using Domain.Entities;

namespace Application.Products.Search
{
    internal class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, List<ProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        public SearchProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<List<ProductResponse>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.SearchProductsAsync(request.Name, request.MinPrice, request.MaxPrice, request.MinQuantity, request.MaxQuantity, cancellationToken);
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
