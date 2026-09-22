using IdentityService.Application.Common.Models;
using IdentityService.Application.Features.Users.Interfaces;
using IdentityService.Application.Features.Users.Models;
using IdentityService.Infrastructure.Mappings;
using IdentityService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

using DomainUser = IdentityService.Domain.Entities.User;
using PersistenceUser = IdentityService.Infrastructure.Persistence.Models.User;

namespace IdentityService.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly IdentityServiceDbContext _dbContext;

        public UserRepository(IdentityServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DomainUser> AddAsync(DomainUser user, CancellationToken cancellationToken = default)
        {
            var persistenceUser = new PersistenceUser
            {
                UserId = user.UserId,
                Username = user.Username,
                DisplayName = user.DisplayName,
                EmailAddress = user.EmailAddress,
                DepartmentCode = user.DepartmentCode,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            await _dbContext.Users.AddAsync(
                persistenceUser,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return UserMapper.ToDomain(persistenceUser);
        }

        public async Task DeleteAsync(DomainUser user, CancellationToken cancellationToken = default)
        {
            var persistenceUser = await _dbContext.Users
                .FirstOrDefaultAsync(
                    x => x.UserId == user.UserId,
                    cancellationToken);

            if (persistenceUser is null)
            {
                return;
            }

            _dbContext.Users.Remove(persistenceUser);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        public Task<bool> EmailAddressExistsAsync(string emailAddress, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            return _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.EmailAddress == emailAddress &&
                        (!excludeUserId.HasValue ||
                         x.UserId != excludeUserId.Value),
                    cancellationToken);
        }

        public async Task<IReadOnlyList<DomainUser>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _dbContext.Users
                .AsNoTracking()
                .OrderBy(x => x.Username)
                .ToListAsync(cancellationToken);

            return users
                .Select(UserMapper.ToDomain)
                .ToList();
        }

        public async Task<DomainUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

            return user is null
                ? null
                : UserMapper.ToDomain(user);
        }

        public async Task<PagedResult<DomainUser>> GetPagedAsync(UserQuery query, CancellationToken cancellationToken = default)
        {
            var usersQuery = _dbContext.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                usersQuery = usersQuery.Where(x =>
                    EF.Functions.ILike(x.Username, $"%{search}%") ||
                    EF.Functions.ILike(x.DisplayName, $"%{search}%") ||
                    EF.Functions.ILike(x.EmailAddress, $"%{search}%"));
            }

            if (!string.IsNullOrWhiteSpace(query.DepartmentCode))
            {
                usersQuery = usersQuery.Where(
                    x => x.DepartmentCode == query.DepartmentCode);
            }

            if (query.IsActive.HasValue)
            {
                usersQuery = usersQuery.Where(
                    x => x.IsActive == query.IsActive.Value);
            }

            var totalItems =
                await usersQuery.CountAsync(cancellationToken);

            var isDescending =
                query.SortDirection.Equals(
                    "desc",
                    StringComparison.OrdinalIgnoreCase);

            usersQuery = query.SortBy?.ToLowerInvariant() switch
            {
                "username" => isDescending
                    ? usersQuery.OrderByDescending(x => x.Username)
                    : usersQuery.OrderBy(x => x.Username),

                "displayname" => isDescending
                    ? usersQuery.OrderByDescending(x => x.DisplayName)
                    : usersQuery.OrderBy(x => x.DisplayName),

                "emailaddress" => isDescending
                    ? usersQuery.OrderByDescending(x => x.EmailAddress)
                    : usersQuery.OrderBy(x => x.EmailAddress),

                "departmentcode" => isDescending
                    ? usersQuery.OrderByDescending(x => x.DepartmentCode)
                    : usersQuery.OrderBy(x => x.DepartmentCode),

                "isactive" => isDescending
                    ? usersQuery.OrderByDescending(x => x.IsActive)
                    : usersQuery.OrderBy(x => x.IsActive),

                _ => usersQuery.OrderBy(x => x.Username)
            };

            var pageIndex = (query.Page - 1) < 0 ? 0 : query.Page;

            var users = await usersQuery
                .Skip(pageIndex * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<DomainUser>
            {
                Items = users.Select(UserMapper.ToDomain).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize)
            };
        }

        public async Task UpdateAsync(DomainUser user, CancellationToken cancellationToken = default)
        {
            var persistenceUser = await _dbContext.Users
                .FirstOrDefaultAsync(
                    x => x.UserId == user.UserId,
                    cancellationToken);

            if (persistenceUser is null)
            {
                return;
            }

            UserMapper.MapToPersistence(
                user,
                persistenceUser);

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        public Task<bool> UsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            return _dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Username == username &&
                        (!excludeUserId.HasValue ||
                         x.UserId != excludeUserId.Value),
                    cancellationToken);

        }

        public async Task<DomainUser?> GetByUsernameAsync(
    string username,
    CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Username == username,
                    cancellationToken);

            return user is null ? null: UserMapper.ToDomain(user);
        }
    }
}
