using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class UserNotFoundByEmailException : Exception
    {
        public UserNotFoundByEmailException(string Email) : base($"User with email {Email} was not found.")
        {
        }
    }
}
