using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Users.Commands.UpdateProfile
{

	public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
	{
		public UpdateProfileCommandValidator()
		{
			RuleFor(x => x.Name).MaximumLength(100).When(x => x.Name is not null);
			RuleFor(x => x.About).MaximumLength(200).When(x => x.About is not null);
		}
	}

}
