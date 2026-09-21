using IdentityService.Application.Common.Models;
using IdentityService.Application.Features.Users.Models;
using IdentityService.Domain.Entities;
using DomainUser = IdentityService.Domain.Entities.User;


namespace IdentityService.Application.Features.Users.Interfaces
{
    public interface IUserRepository
    {
        Task<IReadOnlyList<DomainUser>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<PagedResult<User>> GetPagedAsync(
            UserQuery query,
            CancellationToken cancellationToken = default);

        Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<bool> UsernameExistsAsync(
            string username,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default);

        Task<bool> EmailAddressExistsAsync(
            string emailAddress,
            Guid? excludeUserId = null,
            CancellationToken cancellationToken = default);

        Task<User> AddAsync(
            User user,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            User user,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            User user,
            CancellationToken cancellationToken = default);
    }
}
