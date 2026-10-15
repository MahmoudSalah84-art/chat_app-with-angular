using FluentValidation;

namespace chatme.Application.Features.Push.Commands.UnsubscribePush
{
	public sealed class UnsubscribePushCommandValidator : AbstractValidator<UnsubscribePushCommand>
	{
		public UnsubscribePushCommandValidator() => RuleFor(x => x.Endpoint).NotEmpty().MaximumLength(2048);
	}
}
