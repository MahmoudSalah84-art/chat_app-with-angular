using chatme.Domain.Common;
using chatme.Domain.Enums;

namespace chatme.Application.Common.Interfaces
{
	public sealed record StoredFile(string Url, string PublicId);

	public interface IFileStorageService
	{
		Task<Result<StoredFile>> UploadAsync(
			Stream content, string fileName, MessageType type, CancellationToken cancellationToken = default);

		Task DeleteAsync(string publicId, MessageType type, CancellationToken cancellationToken = default);
	}
}