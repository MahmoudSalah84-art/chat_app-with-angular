using FluentValidation;

namespace chatme.Application.Features.Auth.Commands.RefreshToken
{
	public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
	{
		public RefreshTokenCommandValidator()
		{
			RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(256);
		}
	}
}
