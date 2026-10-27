using chatme.Application.Common.DTO;
using chatme.Application.Common.Interfaces;
using chatme.Application.Features.Users.Commands.UpdateProfile;
using chatme.Application.Tests.Helpers;
using chatme.Domain.Common;
using chatme.Domain.Enums;
using NSubstitute;

namespace chatme.Application.Tests.Handlers.UserHandlerTests
{
	[TestFixture]
	public class UpdateProfileCommandHandlerTests
	{
		private IIdentityService _identity = null!;
		private ICurrentUserService _current = null!;
		private UpdateProfileCommandHandler _sut = null!;

		[SetUp]
		public void SetUp()
		{
			_identity = Substitute.For<IIdentityService>();
			_current = Substitute.For<ICurrentUserService>();
			_sut = new UpdateProfileCommandHandler(_identity, _current);
		}

		[Test]
		public async Task Not_logged_in_returns_unauthorized()
		{
			_current.UserId.Returns((Guid?)null);

			var result = await _sut.Handle(new UpdateProfileCommand("n", null, null), CancellationToken.None);

			ResultAssert.IsFailure(result, ResultErrorType.Unauthorized);
			await _identity.DidNotReceiveWithAnyArgs().UpdateProfileAsync(default, default, default, default, default);
		}

		[Test]
		public async Task Delegates_fields_to_identity()
		{
			var id = Guid.NewGuid();
			_current.UserId.Returns(id);
			var updated = TestData.User(id, "New");
			_identity.UpdateProfileAsync(id, "New", "about", "url", Arg.Any<CancellationToken>()).Returns(Result<UserDto>.Success(updated));

			var result = await _sut.Handle(new UpdateProfileCommand("New", "about", "url"), CancellationToken.None);

			ResultAssert.IsSuccess(result);
			Assert.That(result.Value, Is.SameAs(updated));
		}
	}
}