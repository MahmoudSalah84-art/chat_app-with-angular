using chatme.Application.Common;
using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using MediatR;

namespace chatme.Application.Features.Users.Commands.UploadAvatar;

public sealed class UploadAvatarCommandHandler(
	IFileStorageService fileStorageService,
	IIdentityService identityService,
	ICurrentUserService currentUserService)
	: IRequestHandler<UploadAvatarCommand, Result<UserDto>>
{
	public async Task<Result<UserDto>> Handle(
		UploadAvatarCommand request,
		CancellationToken cancellationToken)
	{
		var userId = currentUserService.UserId;

		if (userId is null)
		{
			return Result<UserDto>.Unauthorized(
				"لازم تسجل دخول الأول");
		}

		if (!AvatarRules.IsAllowedExtension(request.FileName))
		{
			return Result<UserDto>.Failure(
				"الصورة لازم تكون JPG أو PNG أو WebP");
		}

		if (request.Size <= 0 ||
			request.Size > AvatarRules.MaxSizeBytes)
		{
			return Result<UserDto>.Failure(
				"حجم الصورة غير صالح");
		}

		var isValidImage = await AvatarRules.HasImageSignatureAsync(
			request.Content,
			cancellationToken);

		if (!isValidImage)
		{
			return Result<UserDto>.Failure(
				"الملف مش صورة صالحة");
		}

		var upload = await fileStorageService.UploadAvatarAsync(
			request.Content,
			request.FileName,
			userId.Value,
			cancellationToken);

		if (upload.IsFailure)
		{
			return upload.ToFailure<UserDto>();
		}

		return await identityService.UpdateProfileAsync(
			userId.Value,
			null,
			null,
			upload.Value!.Url,
			cancellationToken);
	}
}
