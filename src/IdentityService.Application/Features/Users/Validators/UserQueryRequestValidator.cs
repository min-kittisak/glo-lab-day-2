using FluentValidation;
using IdentityService.Application.Features.Users.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Features.Users.Validators
{
    public sealed class UserQueryRequestValidator : AbstractValidator<UserQueryRequest>
    {
        public UserQueryRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("หน้าต้องมีค่ามากกว่า 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("จำนวนข้อมูลต่อหน้าต้องอยู่ระหว่าง 1 ถึง 100");

            RuleFor(x => x.Search)
                .MaximumLength(200)
                .WithMessage("คำค้นหาต้องมีความยาวไม่เกิน 200 ตัวอักษร")
                .When(x => !string.IsNullOrWhiteSpace(x.Search));

            RuleFor(x => x.DepartmentCode)
                .MaximumLength(50)
                .WithMessage("รหัสแผนกต้องมีความยาวไม่เกิน 50 ตัวอักษร")
                .When(x => !string.IsNullOrWhiteSpace(x.DepartmentCode));

            RuleFor(x => x.SortBy)
            .Must(sortBy =>
                string.IsNullOrWhiteSpace(sortBy) ||
                new[]
                {
                    "username",
                    "displayName",
                    "emailAddress",
                    "departmentCode",
                    "isActive",
                }.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage("SortBy ต้องเป็น username, displayName, emailAddress, departmentCode หรือ isActive");

            RuleFor(x => x.SortDirection)
                .Must(direction =>
                    direction.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                    direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("SortDirection ต้องเป็น asc หรือ desc");
        }
    }
}
