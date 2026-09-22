using System;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Persistence.Models;

/// <summary>
/// ตารางผู้ใช้ตัวอย่างสำหรับ Workshop
/// </summary>
public partial class User
{
    /// <summary>
    /// รหัสผู้ใช้สำหรับชุด Workshop
    /// </summary>
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    /// <summary>
    /// ชื่อแสดงผลสำหรับแบบฝึกหัดค้นหาใน LAB 8 และ LAB 9
    /// </summary>
    public string DisplayName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string? DepartmentCode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// รหัสผ่านแบบเข้ารหัส bcrypt สำหรับผู้ใช้ตัวอย่างของ Workshop
    /// </summary>
    public string PasswordHash { get; set; } = null!;
}
