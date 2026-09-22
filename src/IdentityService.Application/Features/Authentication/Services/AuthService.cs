using FluentValidation;
using IdentityService.Application.Common.Authentication;
using IdentityService.Application.Common.Exceptions;
using IdentityService.Application.Features.Authentication.DTOs;
using IdentityService.Application.Features.Authentication.Interfaces;
using IdentityService.Application.Features.Users.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Features.Authentication.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IValidator<LoginRequest> _validator;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IValidator<LoginRequest> validator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _validator = validator;
        }

        public async Task<LoginResponse> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken = default)
        {
            await _validator.ValidateAndThrowAsync(
                request,
                cancellationToken);

            var user =
                await _userRepository.GetByUsernameAsync(
                    request.Username,
                    cancellationToken);

            if (user is null)
            {
                throw new UnauthorizedException(
                    "ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedException(
                    "บัญชีผู้ใช้งานถูกระงับ");
            }

            var passwordValid =
                _passwordHasher.Verify(
                    request.Password,
                    user.PasswordHash);

            if (!passwordValid)
            {
                throw new UnauthorizedException(
                    "ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง");
            }

            var accessToken =
                _jwtTokenGenerator.GenerateToken(
                    user.UserId,
                    user.Username,
                    user.DisplayName,
                    user.EmailAddress,
                    user.DepartmentCode);

            return new LoginResponse
            {
                AccessToken = accessToken,
                TokenType = "Bearer",
                ExpiresIn = 60 * 60,

                User = new UserInfoResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    EmailAddress = user.EmailAddress,
                    DepartmentCode = user.DepartmentCode
                }
            };
        }
    }
}
