using System.Threading.Channels;
using chatme.Application.Common.Interfaces;

namespace chatme.infrastructure.Services
{
	public sealed class PushJobQueue : IPushJobQueue
	{
		private readonly Channel<MessagePushJob> _channel = Channel.CreateBounded<MessagePushJob>(
			new BoundedChannelOptions(5000) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = false });

		public void Enqueue(MessagePushJob job) => _channel.Writer.TryWrite(job);

		public IAsyncEnumerable<MessagePushJob> ReadAllAsync(CancellationToken cancellationToken) =>
			_channel.Reader.ReadAllAsync(cancellationToken);
	}
}