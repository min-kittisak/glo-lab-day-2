using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Features.Authentication.DTOs
{
    public sealed class LoginResponse
    {
        public string AccessToken { get; init; } = string.Empty;

        public string TokenType { get; init; } = "Bearer";

        public int ExpiresIn { get; init; }

        public UserInfoResponse User { get; init; } = default!;
    }

    public sealed class UserInfoResponse
    {
        public Guid UserId { get; init; }

        public string Username { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string EmailAddress { get; init; } = string.Empty;

        public string? DepartmentCode { get; init; }
    }
}
