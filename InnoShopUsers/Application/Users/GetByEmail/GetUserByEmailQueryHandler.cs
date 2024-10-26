using Application.Data;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.GetByEmail
{
    internal sealed class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserResponse>
    {
        private readonly IApplicationDbContext _context;
        public GetUserByEmailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<UserResponse> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var user = await _context
                .Users
                .Where(u => u.Email == request.Email)
                .Select(u => new UserResponse(u.Id, u.Name, u.Username, u.Email))
                .FirstOrDefaultAsync(cancellationToken);
            if (user is null)
            {
                throw new UserNotFoundByEmailException(request.Email);
            }
            return user;
        }
    }
}
