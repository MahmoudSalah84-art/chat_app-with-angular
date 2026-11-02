namespace chatme.Application.Common.Interfaces
{
	public sealed record PushPayload(
		string Title,
		string Body,
		string? Icon,
		Guid ChatId,
		Guid MessageId,
		string Tag,
		DateTime SentAtUtc,
		string ReceiptToken);

	public interface IWebPushSender
	{
		Task SendAsync(
			Guid userId,
			PushPayload payload,
			CancellationToken cancellationToken = default);
	}
}
