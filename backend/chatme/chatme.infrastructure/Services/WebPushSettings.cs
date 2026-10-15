namespace chatme.infrastructure.Services
{
	public sealed class WebPushSettings
	{
		public const string SectionName = "WebPush";

		public string Subject { get; init; } = string.Empty;
		public string PublicKey { get; init; } = string.Empty;
		public string PrivateKey { get; init; } = string.Empty;

		public bool IsConfigured =>
			!string.IsNullOrWhiteSpace(Subject) &&
			!string.IsNullOrWhiteSpace(PublicKey) &&
			!string.IsNullOrWhiteSpace(PrivateKey);
	}
}