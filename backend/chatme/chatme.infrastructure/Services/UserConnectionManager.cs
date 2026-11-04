using chatme.Application.Common.Interfaces;
using System.Collections.Concurrent;

namespace chatme.infrastructure.Services
{

	public sealed class UserConnectionManager : IUserConnectionManager
	{
		private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>>
			_connections = new();

		public Task AddConnectionAsync(Guid userId, string connectionId)
		{
			var connections = _connections.GetOrAdd(
				userId,
				_ => new ConcurrentDictionary<string, byte>());

			connections.TryAdd(connectionId, 0);

			return Task.CompletedTask;
		}

		public Task RemoveConnectionAsync(Guid userId, string connectionId)
		{
			if (_connections.TryGetValue(userId, out var connections))
			{
				connections.TryRemove(connectionId, out _);

				if (connections.IsEmpty)
					_connections.TryRemove(userId, out _);
			}

			return Task.CompletedTask;
		}

		public Task<bool> HasConnectionsAsync(Guid userId)
		{
			var hasConnections =
				_connections.TryGetValue(userId, out var connections)
				&& !connections.IsEmpty;

			return Task.FromResult(hasConnections);
		}
	}
}
