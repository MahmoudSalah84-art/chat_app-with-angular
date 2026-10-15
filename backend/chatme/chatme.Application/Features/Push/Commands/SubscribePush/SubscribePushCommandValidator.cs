using FluentValidation;
using System.Text.RegularExpressions;

namespace chatme.Application.Features.Push.Commands.SubscribePush
{
	public sealed partial class SubscribePushCommandValidator : AbstractValidator<SubscribePushCommand>
	{
		// for SSRF push endpoints, we only allow a few known hosts (Chrome, Firefox, Edge, Safari)
		private static readonly string[] AllowedHostSuffixes =
		[
			"fcm.googleapis.com",          // Chrome / Brave / Opera / Samsung Internet
			"push.services.mozilla.com",   // Firefox
			"notify.windows.com",          // Edge (WNS)
			"push.apple.com",              // Safari (macOS / iOS)
		];

		public SubscribePushCommandValidator()
		{
			RuleFor(x => x.Endpoint)
				.NotEmpty()
				.MaximumLength(2048)
				.Must(BeAllowedPushEndpoint)
				.WithMessage("الـ endpoint ده مش مسموح");

			RuleFor(x => x.P256dh).NotEmpty().MaximumLength(256).Matches(Base64UrlRegex);
			RuleFor(x => x.Auth).NotEmpty().MaximumLength(64).Matches(Base64UrlRegex);
			RuleFor(x => x.UserAgent).MaximumLength(300);
		}

		private static bool BeAllowedPushEndpoint(string endpoint)
		{
			if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
				return false;

			return AllowedHostSuffixes.Any(suffix =>
				uri.Host.Equals(suffix, StringComparison.OrdinalIgnoreCase) ||
				uri.Host.EndsWith("." + suffix, StringComparison.OrdinalIgnoreCase));
		}
		private static readonly Regex Base64UrlRegex =
			new("^[A-Za-z0-9_-]+$", RegexOptions.Compiled);
	}
}