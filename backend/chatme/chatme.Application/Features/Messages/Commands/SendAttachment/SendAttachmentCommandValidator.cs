using FluentValidation;
using chatme.Application.Common;

namespace chatme.Application.Features.Messages.Commands.SendAttachment
{
	public sealed class SendAttachmentCommandValidator : AbstractValidator<SendAttachmentCommand>
	{
		public SendAttachmentCommandValidator()
		{
			RuleFor(x => x.ChatId).NotEmpty();

			RuleFor(x => x.FileName).NotEmpty()
				.Must(AttachmentRules.IsAllowed).WithMessage("نوع الملف ده مش مسموح");

			RuleFor(x => x.Size)
				.GreaterThan(0).WithMessage("الملف فاضي")
				.LessThanOrEqualTo(AttachmentRules.MaxSizeBytes).WithMessage("حجم الملف لازم يكون أقل من 25 ميجا");

			RuleFor(x => x.Caption).MaximumLength(1000);
		}
	}
}
