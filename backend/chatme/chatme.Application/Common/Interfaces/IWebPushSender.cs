namespace chatme.Application.Common.Interfaces
{
	public sealed record PushPayload(
		string Title,
		string Body,
		string? Icon,
		Guid ChatId,
		Guid MessageId,
		string Tag,
		DateTime SentAtUtc);

	public interface IWebPushSender
	{
		Task SendAsync(IReadOnlyCollection<Guid> userIds, PushPayload payload, CancellationToken cancellationToken = default);
	}
}
