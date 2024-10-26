using MediatR;
using Domain.Entities;
using Application.Data;

namespace Application.Users.Delete
{
    internal class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private IUnitOfWork _unitOfWork;
        public DeleteUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);
            if (user is null)
            {
                throw new UserNotFoundException(request.Id);
            }
            _userRepository.Remove(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
