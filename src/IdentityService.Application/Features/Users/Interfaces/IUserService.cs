using IdentityService.Application.Common.Models;
using IdentityService.Application.Features.Users.DTOs;

namespace IdentityService.Application.Features.Users.Interfaces
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserResponse>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<PagedResult<UserResponse>> GetPagedAsync(
    UserQueryRequest request,
    CancellationToken cancellationToken = default);


        Task<UserResponse> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<UserResponse> CreateAsync(
            CreateUserRequest request,
            CancellationToken cancellationToken = default);

        Task<UserResponse> UpdateAsync(
            Guid userId,
            UpdateUserRequest request,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
