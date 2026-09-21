using System;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Persistence.Models;

/// <summary>
/// Training-only user directory for Identity Workshop
/// </summary>
public partial class User
{
    /// <summary>
    /// Training user identifier
    /// </summary>
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    /// <summary>
    /// Display name used for Lab 8/9 search exercises
    /// </summary>
    public string DisplayName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string? DepartmentCode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
