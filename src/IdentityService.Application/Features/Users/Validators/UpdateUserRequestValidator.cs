using FluentValidation;
using IdentityService.Application.Features.Users.DTOs;

namespace IdentityService.Application.Features.Users.Validators
{
    public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("กรุณาระบุชื่อผู้ใช้งาน")
                .MaximumLength(100)
                .WithMessage("ชื่อผู้ใช้งานต้องมีความยาวไม่เกิน 100 ตัวอักษร");

            RuleFor(x => x.DisplayName)
                .NotEmpty()
                .WithMessage("กรุณาระบุชื่อที่ใช้แสดง")
                .MaximumLength(200)
                .WithMessage("ชื่อที่ใช้แสดงต้องมีความยาวไม่เกิน 200 ตัวอักษร");

            RuleFor(x => x.EmailAddress)
                .NotEmpty()
                .WithMessage("กรุณาระบุอีเมล")
                .EmailAddress()
                .WithMessage("รูปแบบอีเมลไม่ถูกต้อง")
                .MaximumLength(255)
                .WithMessage("อีเมลต้องมีความยาวไม่เกิน 255 ตัวอักษร");

            RuleFor(x => x.DepartmentCode)
                .MaximumLength(50)
                .WithMessage("รหัสแผนกต้องมีความยาวไม่เกิน 50 ตัวอักษร")
                .When(x => !string.IsNullOrWhiteSpace(x.DepartmentCode));
        }
    }
}
