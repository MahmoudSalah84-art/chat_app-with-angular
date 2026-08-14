using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Messages.Commands.SendMessage
{

	public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
	{
		public SendMessageCommandValidator()
		{
			RuleFor(x => x.ChatId).NotEmpty();
			RuleFor(x => x.Content).NotEmpty().WithMessage("مينفعش تبعت رسالة فاضية").MaximumLength(4000);
		}
	}

}
