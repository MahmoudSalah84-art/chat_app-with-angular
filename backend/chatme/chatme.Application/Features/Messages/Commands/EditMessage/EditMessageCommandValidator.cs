using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Messages.Commands.EditMessage
{

	public sealed class EditMessageCommandValidator : AbstractValidator<EditMessageCommand>
	{
		public EditMessageCommandValidator()
		{
			RuleFor(x => x.NewContent).NotEmpty().WithMessage("مينفعش تسيب الرسالة فاضية").MaximumLength(4000);
		}
	}

}
