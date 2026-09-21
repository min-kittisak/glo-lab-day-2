namespace IdentityService.Application.Features.Users.DTOs
{
    public class UpdateUserRequest
    {
        public string Username { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string EmailAddress { get; init; } = string.Empty;

        public string? DepartmentCode { get; init; }

        public bool IsActive { get; init; }
    }
}
