using chatme.Application.Common.DTO;
using chatme.Domain.Common;
using chatme.Domain.Enums;

namespace chatme.Application.Tests.Helpers
{
	internal static class TestData
	{
		public static UserDto User(Guid? id = null, string name = "mahmoud") => new(
		id ?? Guid.NewGuid(), name, $"{name.ToLower()}@test.com", $"https://img/{name}.png",
		null, null, false, null);

		public static AuthResponseDto Auth(UserDto? user = null) => new("jwt-token", user ?? User());

	}


	
}