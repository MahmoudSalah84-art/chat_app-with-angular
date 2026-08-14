using chatme.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace chatme.infrastructure.Services
{
	public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
	{
		public Guid? UserId
		{
			get
			{
				var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
				return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
			}
		}
	}
}
