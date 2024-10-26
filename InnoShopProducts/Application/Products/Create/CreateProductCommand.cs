using MediatR;

namespace Application.Products.Create
{
    public record CreateProductCommand(string Name, string Description, decimal Price, int Quantity, Guid UserId) : IRequest;
    public record CreateProductRequest(string Name, string Description, decimal Price, int Quantity);
}
