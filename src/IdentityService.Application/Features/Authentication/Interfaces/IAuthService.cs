using IdentityService.Application.Features.Authentication.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Features.Authentication.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default);
    }
}
