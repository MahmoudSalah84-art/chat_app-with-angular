using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Auth.Commands.Register
{
	public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
	{
		public RegisterUserCommandValidator()
		{
			RuleFor(x => x.Name).NotEmpty().WithMessage("الاسم مطلوب").MaximumLength(100);
			RuleFor(x => x.Email).NotEmpty().WithMessage("الإيميل مطلوب").EmailAddress().WithMessage("صيغة الإيميل مش صحيحة");
			RuleFor(x => x.Password).NotEmpty().WithMessage("كلمة السر مطلوبة").MinimumLength(6).WithMessage("كلمة السر لازم تكون 6 حروف على الأقل");
		}
	}
}
