using MediatR;

namespace Application.Products.Search
{
    public record SearchProductsQuery(string? Name,
            decimal? MinPrice,
            decimal? MaxPrice,
            int? MinQuantity = null,
            int? MaxQuantity = null) : IRequest<List<ProductResponse>> { }
    public record ProductResponse(Guid Id, string Name, string Description, decimal Price, int Quantity, Guid UserId, DateTime CreatedAt);
}
