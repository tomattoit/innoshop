using MediatR;

namespace Application.Products.GetAll
{
    public class GetAllProductsQuery : IRequest<List<ProductResponse>> { }
    public record ProductResponse(Guid Id, string Name, string Description, decimal Price, int Quantity, Guid UserId, DateTime CreatedAt);
}
