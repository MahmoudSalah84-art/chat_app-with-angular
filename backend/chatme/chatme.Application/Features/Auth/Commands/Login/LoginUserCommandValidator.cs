using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Auth.Commands.Login
{

	public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
	{
		public LoginUserCommandValidator()
		{
			RuleFor(x => x.Email).NotEmpty().EmailAddress();
			RuleFor(x => x.Password).NotEmpty();
		}
	}

}
