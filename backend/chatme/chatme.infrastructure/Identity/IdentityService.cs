using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace chatme.infrastructure.Identity
{
	public sealed class IdentityService(
	UserManager<ApplicationUser> userManager,
	IJwtTokenService jwtTokenService) : IIdentityService
	{
		public async Task<Result<AuthResponseDto>> RegisterAsync(
			string name, string email, string password, CancellationToken cancellationToken = default)
		{
			var existingUser = await userManager.FindByEmailAsync(email);
			if (existingUser is not null)
				return Result<AuthResponseDto>.Conflict("فيه حساب بالإيميل ده بالفعل");

			var user = new ApplicationUser
			{
				Id = Guid.NewGuid(),
				UserName = email,
				Email = email,
				Name = name.Trim(),
				AvatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=25D366&color=fff",
				About = "متاح",
				CreatedAt = DateTime.UtcNow,
			};

			// UserManager.CreateAsync هي اللي بتعمل كل حاجة: تشفير كلمة السر،
			// التحقق من قواعد التعقيد المضبوطة في AddIdentityCore (شوف
			// DependencyInjection.cs)، وحفظ المستخدم في قاعدة البيانات
			var createResult = await userManager.CreateAsync(user, password);
			if (!createResult.Succeeded)
				return Result<AuthResponseDto>.Failure(createResult.Errors.Select(e => e.Description));

			var userDto = MapToDto(user);
			var token = jwtTokenService.GenerateToken(userDto);

			return Result<AuthResponseDto>.Success(new AuthResponseDto(token, userDto));
		}

		public async Task<Result<AuthResponseDto>> LoginAsync(
			string email, string password, CancellationToken cancellationToken = default)
		{
			var user = await userManager.FindByEmailAsync(email);
			if (user is null)
				return Result<AuthResponseDto>.Unauthorized("الإيميل أو كلمة السر غلط");

			var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
			if (!isPasswordValid)
				return Result<AuthResponseDto>.Unauthorized("الإيميل أو كلمة السر غلط");

			var userDto = MapToDto(user);
			var token = jwtTokenService.GenerateToken(userDto);

			return Result<AuthResponseDto>.Success(new AuthResponseDto(token, userDto));
		}

		public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			var user = await userManager.FindByIdAsync(userId.ToString());
			return user is null
				? Result<UserDto>.NotFound("المستخدم مش موجود")
				: Result<UserDto>.Success(MapToDto(user));
		}

		public async Task<Result<UserDto>> UpdateProfileAsync(
			Guid userId, string? name, string? about, string? avatarUrl, CancellationToken cancellationToken = default)
		{
			var user = await userManager.FindByIdAsync(userId.ToString());
			if (user is null)
				return Result<UserDto>.NotFound("المستخدم مش موجود");

			if (!string.IsNullOrWhiteSpace(name)) user.Name = name.Trim();
			if (about is not null) user.About = about.Trim();
			if (!string.IsNullOrWhiteSpace(avatarUrl)) user.AvatarUrl = avatarUrl;

			var updateResult = await userManager.UpdateAsync(user);
			if (!updateResult.Succeeded)
				return Result<UserDto>.Failure(updateResult.Errors.Select(e => e.Description));

			return Result<UserDto>.Success(MapToDto(user));
		}

		public async Task<Result> SetOnlineStatusAsync(Guid userId, bool isOnline, CancellationToken cancellationToken = default)
		{
			var user = await userManager.FindByIdAsync(userId.ToString());
			if (user is null) return Result.Success();

			user.IsOnline = isOnline;
			if (!isOnline) user.LastSeenAt = DateTime.UtcNow;

			await userManager.UpdateAsync(user);
			return Result.Success();
		}

		public Task<List<UserDto>> GetContactsAsync(Guid excludeUserId, CancellationToken cancellationToken = default) =>
			userManager.Users
				.Where(u => u.Id != excludeUserId)
				.Select(u => new UserDto(u.Id, u.Name, u.Email!, u.AvatarUrl, u.About, u.PhoneNumber, u.IsOnline, u.LastSeenAt))
				.ToListAsync(cancellationToken);

		public Task<List<UserDto>> GetUsersByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
			userManager.Users
				.Where(u => ids.Contains(u.Id))
				.Select(u => new UserDto(u.Id, u.Name, u.Email!, u.AvatarUrl, u.About, u.PhoneNumber, u.IsOnline, u.LastSeenAt))
				.ToListAsync(cancellationToken);

		private static UserDto MapToDto(ApplicationUser user) => new(
			user.Id, user.Name, user.Email ?? string.Empty, user.AvatarUrl,
			user.About, user.PhoneNumber, user.IsOnline, user.LastSeenAt);
	}
}