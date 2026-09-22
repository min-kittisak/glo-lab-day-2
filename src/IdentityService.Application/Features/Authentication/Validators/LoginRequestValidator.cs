using FluentValidation;
using IdentityService.Application.Features.Authentication.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Features.Authentication.Validators
{
    public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("กรุณาระบุชื่อผู้ใช้งาน");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("กรุณาระบุรหัสผ่าน");
        }
    }
}
