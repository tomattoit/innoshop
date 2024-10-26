using Domain.Entities;
using MediatR;
using System.Net.Http.Json;


namespace Application.Products.GetWithUser
{
    public sealed class GetProductWithUserQueryHandler : IRequestHandler<GetProductWithUserQuery, ProductWithUserResponse>
    {
        private readonly HttpClient _httpClient;
        private readonly IProductRepository _productRepository;
        public GetProductWithUserQueryHandler(IProductRepository productRepository, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _productRepository = productRepository;
        }
        public async Task<ProductWithUserResponse> Handle(GetProductWithUserQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
            {
                throw new ProductNotFoundException(request.Id);
            }
            var userResponse = await _httpClient.GetFromJsonAsync<UserResponse>($"http://innoshopusers.api:5000/users/{product.UserId}", cancellationToken);
            var result = new ProductWithUserResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Quantity,
                product.CreatedAt,
                userResponse!);
            return result;
        }
    }
}
