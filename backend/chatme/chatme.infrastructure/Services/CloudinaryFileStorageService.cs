using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using chatme.Domain.Enums;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace chatme.infrastructure.Services
{
	public sealed class CloudinaryFileStorageService(
		Cloudinary cloudinary,
		IOptions<CloudinarySettings> options,
		ILogger<CloudinaryFileStorageService> logger) : IFileStorageService
	{
		private readonly CloudinarySettings _settings = options.Value;

		public async Task<Result<StoredFile>> UploadAsync(
			Stream content, string fileName, MessageType type, CancellationToken cancellationToken = default)
		{
			var file = new FileDescription(fileName, content);

			UploadResult result = type switch
			{
				MessageType.Image => await cloudinary.UploadAsync(new ImageUploadParams
				{
					File = file,
					Folder = _settings.Folder,
					UseFilename = true,
					UniqueFilename = true,
					Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
				}, cancellationToken),

				// Video and Audio files are uploaded as videos in Cloudinary
				MessageType.Video or MessageType.Audio => await cloudinary.UploadAsync(new VideoUploadParams
				{
					File = file,
					Folder = _settings.Folder,
					UseFilename = true,
					UniqueFilename = true,
				}, cancellationToken),

				_ => await cloudinary.UploadAsync(new RawUploadParams
				{
					File = file,
					Folder = _settings.Folder,
					UseFilename = true,
					UniqueFilename = true,
				}, cancellationToken : cancellationToken),
			};

			if (result.Error is not null)
			{
				logger.LogError("Cloudinary upload failed: {Message}", result.Error.Message);
				return Result<StoredFile>.Failure("حصلت مشكلة وإحنا بنرفع الملف، جرب تاني");
			}

			return Result<StoredFile>.Success(new StoredFile(result.SecureUrl.ToString(), result.PublicId));
		}

		public async Task DeleteAsync(string publicId, MessageType type, CancellationToken cancellationToken = default)
		{
			var resourceType = type switch
			{
				MessageType.Image => ResourceType.Image,
				MessageType.Video or MessageType.Audio => ResourceType.Video,
				_ => ResourceType.Raw,
			};

			var result = await cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = resourceType });
			if (result.Error is not null)
				logger.LogWarning("Cloudinary delete failed for {PublicId}: {Message}", publicId, result.Error.Message);
		}


		public async Task<Result<StoredFile>> UploadAvatarAsync(
		Stream content,
		string fileName,
		Guid userId,
		CancellationToken cancellationToken = default)
		{
			var uploadParams = new ImageUploadParams
			{
				File = new FileDescription(fileName, content),

				PublicId = $"{_settings.Folder}/avatars/{userId:N}",

				Overwrite = true,

				Invalidate = true,

				AllowedFormats = ["jpg", "jpeg", "png", "webp"],

				Transformation = new Transformation()
					.Width(512)
					.Height(512)
					.Crop("fill")
					.Gravity("face")
					.Quality("auto")
			};

			try
			{
				var result = await cloudinary.UploadAsync(
					uploadParams,
					cancellationToken);

				if (result.Error is not null ||
					string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()))
				{
					logger.LogError(
						"Cloudinary avatar upload failed for {UserId}: {Message}",
						userId,
						result.Error?.Message ?? "Empty secure URL");

					return Result<StoredFile>.Failure(
						"حصلت مشكلة وإحنا بنرفع الصورة، جرب تاني");
				}

				var storedImage = new StoredFile(
					result.SecureUrl.ToString(),
					result.PublicId);

				return Result<StoredFile>.Success(storedImage);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				logger.LogError(
					ex,
					"Cloudinary avatar upload failed for {UserId}",
					userId);

				return Result<StoredFile>.Failure(
					"حصلت مشكلة وإحنا بنرفع الصورة، جرب تاني");
			}
		}
	}
}