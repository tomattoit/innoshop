using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public interface IUserRepository
    {
        void Add(User user);
        void Update(User user);
        void Remove(User user);
        Task<User?> GetByIdAsync(Guid id);
    }
}
