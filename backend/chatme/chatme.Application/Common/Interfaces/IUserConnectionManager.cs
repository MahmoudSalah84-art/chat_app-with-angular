
namespace chatme.Application.Common.Interfaces
{
	public interface IUserConnectionManager
	{
		Task AddConnectionAsync(Guid userId, string connectionId);

		Task RemoveConnectionAsync(Guid userId, string connectionId);

		Task<bool> HasConnectionsAsync(Guid userId);
	}
}
