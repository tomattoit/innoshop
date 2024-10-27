using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Application.Data;

namespace Application.Users.Get
{
    public sealed class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserResponse>
    {
        private readonly IApplicationDbContext _context;
        public GetUserQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<UserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _context
                .Users
                .Where(u => u.Id == request.Id)
                .Select(u => new UserResponse(u.Id, u.Name, u.Username, u.Email))
                .FirstOrDefaultAsync(cancellationToken);
            if (user is null)
            {
                throw new UserNotFoundException(request.Id);
            }
            return user;
        }
    }
}
