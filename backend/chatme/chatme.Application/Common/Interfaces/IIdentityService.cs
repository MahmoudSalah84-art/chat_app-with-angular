using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.Interfaces
{
	public interface IIdentityService
	{
		Task<Result<AuthResponseDto>> RegisterAsync(string name, string email, string password, CancellationToken cancellationToken = default);
		Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
		Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
		Task<Result<UserDto>> UpdateProfileAsync(Guid userId, string? name, string? about, string? avatarUrl, CancellationToken cancellationToken = default);
		Task<Result> SetOnlineStatusAsync(Guid userId, bool isOnline, CancellationToken cancellationToken = default);
		Task<List<UserDto>> GetContactsAsync(Guid excludeUserId, CancellationToken cancellationToken = default);
		Task<List<UserDto>> GetUsersByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
	}

}
