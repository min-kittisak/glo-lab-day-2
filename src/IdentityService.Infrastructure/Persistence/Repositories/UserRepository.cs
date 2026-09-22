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

            //===== FromRawSql =====
            // await _dbContext.Database
            //.ExecuteSqlInterpolatedAsync($"""
            //            INSERT INTO workshop.users
            //            (
            //                user_id,
            //                username,
            //                display_name,
            //                email_address,
            //                department_code,
            //                is_active,
            //                created_at
            //            )
            //            VALUES
            //            (
            //                {persistenceUser.UserId},
            //                {persistenceUser.Username},
            //                {persistenceUser.DisplayName},
            //                {persistenceUser.EmailAddress},
            //                {persistenceUser.DepartmentCode},
            //                {persistenceUser.IsActive},
            //                {persistenceUser.CreatedAt}
            //            )
            //            """,
            //    cancellationToken);


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

            //===== FromRawSql =====
            //await _dbContext.Database
            //.ExecuteSqlInterpolatedAsync($"""
            //    DELETE FROM workshop.users
            //    WHERE user_id = {persistenceUser.UserId}
            //    """,
            //    cancellationToken);
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

            //===== FromRawSql =====
            //var user = await _dbContext.Users
            //        .FromSqlInterpolated($"""
            //                    SELECT
            //                        user_id,
            //                        username,
            //                        display_name,
            //                        email_address,
            //                        department_code,
            //                        is_active,
            //                        created_at
            //                    FROM workshop.users
            //                    WHERE email_address = {emailAddress}
            //                    LIMIT 1
            //                    """)
            //        .AsNoTracking()
            //        .FirstOrDefaultAsync(cancellationToken);

            //return user is not null;
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

            //===== FromRawSql =====
            //var users = await _dbContext.Users
            //    .FromSqlRaw(
            //        """
            //SELECT
            //    user_id,
            //    username,
            //    display_name,
            //    email_address,
            //    department_code,
            //    is_active,
            //    created_at
            //FROM workshop.users
            //WHERE department_code = {0}
            //ORDER BY username
            //""",
            //        departmentCode)
            //    .AsNoTracking()
            //    .ToListAsync(cancellationToken);

            //return users
            //    .Select(UserMapper.ToDomain)
            //    .ToList();
        }

        public async Task<DomainUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

            //===== FromRawSql =====
            //var user = await _dbContext.Users
            //    .FromSqlInterpolated($"""
            //                SELECT
            //                    user_id,
            //                    username,
            //                    display_name,
            //                    email_address,
            //                    department_code,
            //                    is_active,
            //                    created_at
            //                FROM workshop.users
            //                WHERE user_id = {userId}
            //                """)
            //    .AsNoTracking()
            //    .FirstOrDefaultAsync(cancellationToken);

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


            //===== FromRawSql =====
            //var searchPattern = string.IsNullOrWhiteSpace(query.Search) ? null : $"%{query.Search.Trim()}%";

            //var departmentCode = string.IsNullOrWhiteSpace(query.DepartmentCode) ? null: query.DepartmentCode.Trim();

            //var offset = (query.Page - 1) * query.PageSize;

            //var sortBy =query.SortBy?.ToLowerInvariant();

            //var isDescending = query.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

            //FormattableString sql = (sortBy, isDescending) switch
            //{
            //    ("username", false) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY username ASC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("username", true) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY username DESC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("displayname", false) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY display_name ASC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("displayname", true) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY display_name DESC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("emailaddress", false) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY email_address ASC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("emailaddress", true) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY email_address DESC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("departmentcode", false) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY department_code ASC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("departmentcode", true) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY department_code DESC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("isactive", false) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY is_active ASC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("isactive", true) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY is_active DESC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    ("createdat", true) => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY created_at DESC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """,

            //    _ => $"""
            //        SELECT
            //            user_id,
            //            username,
            //            display_name,
            //            email_address,
            //            department_code,
            //            is_active,
            //            created_at
            //        FROM workshop.users
            //        WHERE
            //            (
            //                CAST({searchPattern} AS text) IS NULL
            //                OR username ILIKE {searchPattern}
            //                OR display_name ILIKE {searchPattern}
            //                OR email_address ILIKE {searchPattern}
            //            )
            //            AND (
            //                CAST({departmentCode} AS text) IS NULL
            //                OR department_code = {departmentCode}
            //            )
            //            AND (
            //                CAST({query.IsActive} AS boolean) IS NULL
            //                OR is_active = {query.IsActive}
            //            )
            //        ORDER BY created_at ASC
            //        LIMIT {query.PageSize}
            //        OFFSET {offset}
            //        """
            //        };

            //var users = await _dbContext.Users
            //    .FromSqlInterpolated(sql)
            //    .AsNoTracking()
            //    .ToListAsync(cancellationToken);

            //var totalItems = await _dbContext.Users
            //    .AsNoTracking()
            //    .Where(x =>
            //        (searchPattern == null ||
            //         EF.Functions.ILike(
            //             x.Username,
            //             searchPattern) ||
            //         EF.Functions.ILike(
            //             x.DisplayName,
            //             searchPattern) ||
            //         EF.Functions.ILike(
            //             x.EmailAddress,
            //             searchPattern))
            //        &&
            //        (departmentCode == null ||
            //         x.DepartmentCode == departmentCode)
            //        &&
            //        (!query.IsActive.HasValue ||
            //         x.IsActive == query.IsActive.Value))
            //    .CountAsync(cancellationToken);

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

            //===== FromRawSql =====
            //await _dbContext.Database
            //.ExecuteSqlInterpolatedAsync($"""
            //    UPDATE workshop.users
            //    SET
            //        username = {user.Username},
            //        display_name = {user.DisplayName},
            //        email_address = {user.EmailAddress},
            //        department_code = {user.DepartmentCode},
            //        is_active = {user.IsActive}
            //    WHERE user_id = {user.UserId}
            //    """,
            //    cancellationToken);
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

            //===== FromRawSql =====
            //var user = await _dbContext.Users
            //.FromSqlInterpolated($"""
            //            SELECT
            //                user_id,
            //                username,
            //                display_name,
            //                email_address,
            //                department_code,
            //                is_active,
            //                created_at
            //            FROM workshop.users
            //            WHERE username = {username}
            //            LIMIT 1
            //            """)
            //.AsNoTracking()
            //.FirstOrDefaultAsync(cancellationToken);

            //return user is not null;
        }
    }
}
