using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Features.Authentication.DTOs
{
    public sealed class LoginRequest
    {
        public string Username { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }
}
