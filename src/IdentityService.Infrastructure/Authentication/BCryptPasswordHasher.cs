using IdentityService.Application.Common.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Authentication
{
    public sealed class BCryptPasswordHasher : IPasswordHasher
    {
        public bool Verify(
            string password,
            string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
