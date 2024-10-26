using MediatR;

namespace Application.Products.Delete
{
    public record DeleteProductCommand(Guid Id, Guid UserId) : IRequest;
}
