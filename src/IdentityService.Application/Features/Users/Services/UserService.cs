using FluentValidation;
using IdentityService.Application.Common.Authentication;
using IdentityService.Application.Common.Exceptions;
using IdentityService.Application.Common.Models;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Features.Users.Interfaces;
using IdentityService.Application.Features.Users.Models;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Features.Users.Services
{
    public sealed class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IValidator<UserQueryRequest> _queryValidator;
        private readonly IValidator<CreateUserRequest> _createValidator;
        private readonly IValidator<UpdateUserRequest> _updateValidator;
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordHasher _passwordHasher;


        public UserService(
            IUserRepository userRepository,
            IValidator<UserQueryRequest> queryValidator,
            IValidator<CreateUserRequest> createValidator,
            IValidator<UpdateUserRequest> updateValidator,
            ILogger<UserService> logger,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _queryValidator = queryValidator;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var username = request.Username.Trim();

            var displayName = request.DisplayName.Trim();

            var emailAddress = request.EmailAddress.Trim().ToLowerInvariant();

            var departmentCode = string.IsNullOrWhiteSpace(request.DepartmentCode) ? null : request.DepartmentCode.Trim();

            var usernameExists = await _userRepository.UsernameExistsAsync(username, cancellationToken: cancellationToken);

            if (usernameExists)
            {
                throw new ConflictException("ชื่อผู้ใช้งานนี้ถูกใช้งานแล้ว");
            }

            var emailExists = await _userRepository.EmailAddressExistsAsync(emailAddress, cancellationToken: cancellationToken);

            if (emailExists)
            {
                throw new ConflictException("อีเมลนี้ถูกใช้งานแล้ว");
            }

            var user = new User(
                username,
                displayName,
                emailAddress,
                departmentCode);

            var hashPassword = _passwordHasher.Hash(request.Password);

            user.SetPasswordHash(hashPassword);

            _logger.LogInformation("กำลังสร้างผู้ใช้งาน Username: {Username}, DepartmentCode: {DepartmentCode}",username, departmentCode);
            var createdUser = await _userRepository.AddAsync(user, cancellationToken);
            _logger.LogInformation("สร้างผู้ใช้งานสำเร็จ UserId: {UserId}, Username: {Username}", createdUser.UserId, createdUser.Username);

            return UserResponse.FromEntity(createdUser);
        }

        public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user =
    await _userRepository.GetByIdAsync(
        userId,
        cancellationToken);

            if (user is null)
            {
                throw new NotFoundException(
                    $"ไม่พบข้อมูลผู้ใช้งานรหัส {userId}");
            }

            await _userRepository.DeleteAsync(
                user,
                cancellationToken);
        }

        public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(UserResponse.FromEntity).ToList();
        }

        public async Task<PagedResult<UserResponse>> GetPagedAsync(
            UserQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            await _queryValidator.ValidateAndThrowAsync(
                request,
                cancellationToken);

            var query = new UserQuery
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Search = string.IsNullOrWhiteSpace(request.Search)
                    ? null
                    : request.Search.Trim(),
                DepartmentCode =
                    string.IsNullOrWhiteSpace(request.DepartmentCode)
                        ? null
                        : request.DepartmentCode.Trim(),
                IsActive = request.IsActive,

                SortBy = string.IsNullOrWhiteSpace(request.SortBy) ? null : request.SortBy.Trim(),

                SortDirection = request.SortDirection.Trim()
            };

            var result = await _userRepository.GetPagedAsync(query, cancellationToken);

            return new PagedResult<UserResponse>
            {
                Items = result.Items
                    .Select(UserResponse.FromEntity)
                    .ToList(),

                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
        }

        public async Task<UserResponse> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException($"ไม่พบข้อมูลผู้ใช้งานรหัส {userId}");
            }

            return UserResponse.FromEntity(user);
        }

        public async Task<UserResponse> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            await _updateValidator.ValidateAndThrowAsync(
    request,
    cancellationToken);

            var user =
                await _userRepository.GetByIdAsync(
                    userId,
                    cancellationToken);

            if (user is null)
            {
                throw new NotFoundException(
                    $"ไม่พบข้อมูลผู้ใช้งานรหัส {userId}");
            }

            var username = request.Username.Trim();

            var displayName = request.DisplayName.Trim();

            var emailAddress = request.EmailAddress
                .Trim()
                .ToLowerInvariant();

            var departmentCode =
                string.IsNullOrWhiteSpace(request.DepartmentCode)
                    ? null
                    : request.DepartmentCode.Trim();

            var usernameExists =
                await _userRepository.UsernameExistsAsync(
                    username,
                    userId,
                    cancellationToken);

            if (usernameExists)
            {
                throw new ConflictException(
                    "ชื่อผู้ใช้งานนี้ถูกใช้งานแล้ว");
            }

            var emailExists =
                await _userRepository.EmailAddressExistsAsync(
                    emailAddress,
                    userId,
                    cancellationToken);

            if (emailExists)
            {
                throw new ConflictException(
                    "อีเมลนี้ถูกใช้งานแล้ว");
            }

            user.Update(
                username,
                displayName,
                emailAddress,
                departmentCode,
                request.IsActive);

            await _userRepository.UpdateAsync(
                user,
                cancellationToken);

            return UserResponse.FromEntity(user);
        }
    }
}
