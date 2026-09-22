using IdentityService.Application.Features.Authentication.DTOs;
using IdentityService.Application.Features.Authentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var response =
                await _authService.LoginAsync(
                    request,
                    cancellationToken);

            return Ok(response);
        }
    }
}
