using Domain.Entities;

namespace Application.Authentication
{
    public interface ITokenProvider
    {
        string Create(User users);
    }
}
