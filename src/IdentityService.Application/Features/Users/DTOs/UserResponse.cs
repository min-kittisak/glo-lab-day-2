using IdentityService.Domain.Entities;

namespace IdentityService.Application.Features.Users.DTOs
{
    public sealed class UserResponse
    {
        public Guid UserId { get; init; }

        public string Username { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string EmailAddress { get; init; } = string.Empty;

        public string? DepartmentCode { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }


        public static UserResponse FromEntity(User user)
        {
            return new UserResponse
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

    }
}
