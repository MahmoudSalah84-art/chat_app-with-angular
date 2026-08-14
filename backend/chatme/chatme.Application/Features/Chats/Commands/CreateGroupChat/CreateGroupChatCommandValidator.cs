using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Chats.Commands.CreateGroupChat
{

	public sealed class CreateGroupChatCommandValidator : AbstractValidator<CreateGroupChatCommand>
	{
		public CreateGroupChatCommandValidator()
		{
			RuleFor(x => x.Name).NotEmpty().WithMessage("اسم المجموعة مطلوب").MaximumLength(100);
			RuleFor(x => x.MemberIds).NotEmpty().WithMessage("لازم تختار عضو واحد على الأقل");
		}
	}

}
