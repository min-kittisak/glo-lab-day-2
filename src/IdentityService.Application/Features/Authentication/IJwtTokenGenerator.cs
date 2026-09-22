using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Features.Authentication
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(
            Guid userId,
            string username,
            string displayName,
            string emailAddress,
            string? departmentCode);
    }
}
