using System;
using System.Collections.Generic;
using IdentityService.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Context;

public partial class IdentityServiceDbContext : DbContext
{
    public IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users", "workshop", tb => tb.HasComment("ตารางผู้ใช้ตัวอย่างสำหรับ Workshop"));

            entity.HasIndex(e => e.Username, "uq_workshop_users_username").IsUnique();

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasComment("รหัสผู้ใช้สำหรับชุด Workshop")
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentCode)
                .HasMaxLength(30)
                .HasColumnName("department_code");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(200)
                .HasComment("ชื่อแสดงผลสำหรับแบบฝึกหัดค้นหาใน LAB 8 และ LAB 9")
                .HasColumnName("display_name");
            entity.Property(e => e.EmailAddress)
                .HasMaxLength(255)
                .HasColumnName("email_address");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(100)
                .HasDefaultValueSql("'$2a$10$HN1PxLf/Es1Df9n/LTVLAezoFi7GttOr4LeZg3bjPXFKkJcySd3Wy'::character varying")
                .HasComment("รหัสผ่านแบบเข้ารหัส bcrypt สำหรับผู้ใช้ตัวอย่างของ Workshop")
                .HasColumnName("password_hash");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
