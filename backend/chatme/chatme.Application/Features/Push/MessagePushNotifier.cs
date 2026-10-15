using chatme.Application.Common.Interfaces;
using chatme.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace chatme.Application.Features.Push
{
	public sealed class MessagePushNotifier(
		IApplicationDbContext dbContext,
		IIdentityService identityService,
		IWebPushSender webPushSender)
	{
		private const int MaxPreviewLength = 140;

		public async Task NotifyAsync(MessagePushJob job, CancellationToken cancellationToken)
		{
			var message = await dbContext.MessagesReadOnly
				.Where(m => m.Id == job.MessageId)
				.Select(m => new { m.Type, m.Content, m.SentAt, m.IsDeleted })
				.FirstOrDefaultAsync(cancellationToken);

			if (message is null || message.IsDeleted)
				return;

			var chat = await dbContext.Chats
				.AsNoTracking()
				.Where(c => c.Id == job.ChatId)
				.Select(c => new
				{
					c.IsGroup,
					c.Name,
					c.AvatarUrl,
					ParticipantIds = c.Participants.Select(p => p.UserId).ToList(),
				})
				.FirstOrDefaultAsync(cancellationToken);

			if (chat is null)
				return;

			var recipients = chat.ParticipantIds.Where(id => id != job.SenderId).ToList();
			if (recipients.Count == 0)
				return;

			var sender = (await identityService.GetUsersByIdsAsync([job.SenderId], cancellationToken)).FirstOrDefault();
			var senderName = sender?.Name ?? "رسالة جديدة";
			var preview = BuildPreview(message.Type, message.Content);

			var payload = new PushPayload(
				Title: chat.IsGroup ? (chat.Name ?? "مجموعة") : senderName,
				Body: chat.IsGroup ? $"{senderName}: {preview}" : preview,
				Icon: chat.IsGroup ? chat.AvatarUrl : sender?.AvatarUrl,
				ChatId: job.ChatId,
				MessageId: job.MessageId,
				Tag: $"chat-{job.ChatId}",
				SentAtUtc: DateTime.SpecifyKind(message.SentAt, DateTimeKind.Utc));

			await webPushSender.SendAsync(recipients, payload, cancellationToken);
		}

		private static string BuildPreview(MessageType type, string content) => type switch
		{
			MessageType.Text => content.Length <= MaxPreviewLength ? content : content[..MaxPreviewLength] + "…",
			MessageType.Image => "📷 صورة",
			_ => "📎 مرفق",
		};
	}
}