using Domain.Entities;
using MediatR;

namespace Application.Products.Update
{
    public record UpdateProductCommand(Guid Id, string Name, string Description, decimal Price, int Quantity, Guid UserId) : IRequest;
    public record UpdateProductRequest(string Name, string Description, decimal Price, int Quantity);
}
