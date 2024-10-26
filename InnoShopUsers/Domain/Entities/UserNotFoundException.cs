using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class UserNotFoundException : Exception
    {
        public UserNotFoundException(Guid id) : base($"User with id {id} was not found.")
        {
        }
    }
}
