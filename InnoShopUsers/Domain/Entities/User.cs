using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User
    {
        public User(Guid id, string name, string username, string passwordHash, string email)
        {
            Id = id;
            Name = name;
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
        }
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public string Email { get; private set; }
        public void Update(string name, string username, string password, string email)
        {
            Name = name;
            Username = username;
            PasswordHash = password;
            Email = email;
        }
    }
}
