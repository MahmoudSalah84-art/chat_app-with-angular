using chatme.Application.Common;
using FluentValidation;

namespace chatme.Application.Features.Users.Commands.UploadAvatar;

public sealed class UploadAvatarCommandValidator
	: AbstractValidator<UploadAvatarCommand>
{
	public UploadAvatarCommandValidator()
	{
		RuleFor(x => x.FileName)
			.NotEmpty()
			.Must(AvatarRules.IsAllowedExtension)
			.WithMessage(
				"الصورة لازم تكون JPG أو PNG أو WebP");

		RuleFor(x => x.Size)
			.GreaterThan(0)
			.WithMessage("الملف فاضي")
			.LessThanOrEqualTo(AvatarRules.MaxSizeBytes)
			.WithMessage(
				"حجم الصورة لازم يكون 5 ميجابايت أو أقل");

		RuleFor(x => x.Content)
			.NotNull()
			.WithMessage("الصورة مطلوبة");
	}
}
