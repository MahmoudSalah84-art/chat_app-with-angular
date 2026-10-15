namespace chatme.Application.Common.Interfaces
{
	public sealed record MessagePushJob(Guid ChatId, Guid MessageId, Guid SenderId);

	public interface IPushJobQueue
	{
		void Enqueue(MessagePushJob job);
	}
}