using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Application.Features.Users.Queries.GetProfile;
using chatme.Application.Tests.Helpers;
using chatme.Domain.Common;
using chatme.Domain.Enums;
using NSubstitute;

namespace chatme.Application.Tests.Handlers.UserHandlerTests
{
	[TestFixture]
	public class GetCurrentUserProfileQueryHandlerTests
	{
		[Test]
		public async Task Not_logged_in_returns_unauthorized()
		{
			var current = Substitute.For<ICurrentUserService>();
			current.UserId.Returns((Guid?)null);

			var result = await new GetCurrentUserProfileQueryHandler(Substitute.For<IIdentityService>(), current)
				.Handle(new GetCurrentUserProfileQuery(), CancellationToken.None);

			ResultAssert.IsFailure(result, ResultErrorType.Unauthorized);
		}

		[Test]
		public async Task Returns_the_current_users_profile()
		{
			var user = TestData.User();
			var current = Substitute.For<ICurrentUserService>();
			current.UserId.Returns(user.Id);
			var identity = Substitute.For<IIdentityService>();
			identity.GetUserByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(Result<UserDto>.Success(user));

			var result = await new GetCurrentUserProfileQueryHandler(identity, current)
				.Handle(new GetCurrentUserProfileQuery(), CancellationToken.None);

			ResultAssert.IsSuccess(result);
			Assert.That(result.Value, Is.SameAs(user));
		}
	}
}
