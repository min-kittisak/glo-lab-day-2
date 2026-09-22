namespace IdentityService.Domain.Entities
{
    public sealed class User
    {
        public Guid UserId { get; private set; }

        public string Username { get; private set; } = string.Empty;

        public string DisplayName { get; private set; } = string.Empty;

        public string EmailAddress { get; private set; } = string.Empty;

        public string? DepartmentCode { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public string PasswordHash { get; private set; } = string.Empty;

        private User()
        {
        }

        public User(
            string username,
            string displayName,
            string emailAddress,
            string? departmentCode)
        {
            UserId = Guid.NewGuid();
            Username = username;
            DisplayName = displayName;
            EmailAddress = emailAddress;
            DepartmentCode = departmentCode;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public static User Restore(
            Guid userId,
            string username,
            string displayName,
            string emailAddress,
            string? departmentCode,
            bool isActive,
            DateTime createdAt,
            string passwordHash)
        {
            return new User
            {
                UserId = userId,
                Username = username,
                DisplayName = displayName,
                EmailAddress = emailAddress,
                DepartmentCode = departmentCode,
                IsActive = isActive,
                CreatedAt = createdAt,
                PasswordHash = passwordHash
            };
        }

        public void Update(
            string username,
            string displayName,
            string emailAddress,
            string? departmentCode,
            bool isActive)
        {
            Username = username;
            DisplayName = displayName;
            EmailAddress = emailAddress;
            DepartmentCode = departmentCode;
            IsActive = isActive;
        }

        public void SetPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash;
        }
    }
}
