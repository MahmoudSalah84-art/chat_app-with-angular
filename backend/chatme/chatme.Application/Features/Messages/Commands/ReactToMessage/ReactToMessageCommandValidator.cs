using chatme.Domain.Common;
using FluentValidation;

namespace chatme.Application.Features.Messages.Commands.ReactToMessage
{
	public sealed class ReactToMessageCommandValidator : AbstractValidator<ReactToMessageCommand>
	{
		public ReactToMessageCommandValidator()
		{
			RuleFor(x => x.ChatId).NotEmpty();
			RuleFor(x => x.MessageId).NotEmpty();
			RuleFor(x => x.Emoji).NotEmpty().MaximumLength(16)
				.Must(ReactionEmojis.IsAllowed).WithMessage("التفاعل ده مش مسموح");
		}
	}
}