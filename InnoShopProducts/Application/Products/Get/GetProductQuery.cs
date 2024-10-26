using MediatR;

namespace Application.Products.Get
{
    public record GetProductQuery(Guid Id) : IRequest<ProductResponse>;
    public record ProductResponse(Guid Id, string Name, string Description, decimal Price, int Quantity, Guid UserId, DateTime CreatedAt);
}
