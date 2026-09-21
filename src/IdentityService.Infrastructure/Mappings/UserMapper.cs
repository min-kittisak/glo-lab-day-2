using DomainUser = IdentityService.Domain.Entities.User;
using PersistenceUser = IdentityService.Infrastructure.Persistence.Models.User;

namespace IdentityService.Infrastructure.Mappings
{
    public static class UserMapper
    {
        public static DomainUser ToDomain(PersistenceUser user)
        {
            return DomainUser.Restore(
                user.UserId,
                user.Username,
                user.DisplayName,
                user.EmailAddress,
                user.DepartmentCode,
                user.IsActive,
                user.CreatedAt);
        }

        public static PersistenceUser ToPersistence(DomainUser user)
        {
            return new PersistenceUser
            {
                UserId = user.UserId,
                Username = user.Username,
                DisplayName = user.DisplayName,
                EmailAddress = user.EmailAddress,
                DepartmentCode = user.DepartmentCode,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public static void MapToPersistence(
            DomainUser source,
            PersistenceUser destination)
        {
            destination.Username = source.Username;
            destination.DisplayName = source.DisplayName;
            destination.EmailAddress = source.EmailAddress;
            destination.DepartmentCode = source.DepartmentCode;
            destination.IsActive = source.IsActive;
        }
    }
}
