using MediatR;

namespace Application.Products.GetWithUser
{
    public record GetProductWithUserQuery(Guid Id) : IRequest<ProductWithUserResponse>;
    public record UserResponse(
        Guid Id,
        string Name,
        string Username,
        string Email);
    public record ProductWithUserResponse(
        Guid Id, 
        string Name, 
        string Description, 
        decimal Price, 
        int Quantity, 
        DateTime CreatedAt,
        UserResponse User);
}
