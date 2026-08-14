using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace chatme.infrastructure.Services
{
	public sealed class JwtTokenService(IOptions<JwtSettings> jwtSettings) : IJwtTokenService
	{
		private readonly JwtSettings _settings = jwtSettings.Value;

		public string GenerateToken(UserDto user)
		{
			var claims = new[]
			{
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new Claim(ClaimTypes.Email, user.Email),
			new Claim(ClaimTypes.Name, user.Name),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _settings.Issuer,
				audience: _settings.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}


