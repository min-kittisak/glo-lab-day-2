namespace IdentityService.Application.Features.Users.DTOs
{
    public sealed class CreateUserRequest
    {
        public string Username { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string EmailAddress { get; init; } = string.Empty;

        public string Password {  get; init; } = string.Empty;

        public string? DepartmentCode { get; init; }
    }
}
