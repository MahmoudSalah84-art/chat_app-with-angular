using chatme.Application.Common.Interfaces;
using chatme.Application.Features.Users.Commands.SetOnlineStatus;
using chatme.Application.Tests.Helpers;
using chatme.Domain.Common;
using NSubstitute;

namespace chatme.Application.Tests.Handlers.UserHandlerTests
{
	[TestFixture]
	public class SetOnlineStatusCommandHandlerTests
	{
		private IIdentityService _identity = null!;
		private ICurrentUserService _current = null!;
		private IChatNotificationService _notification = null!;
		private SetOnlineStatusCommandHandler _sut = null!;

		[SetUp]
		public void SetUp()
		{
			_identity = Substitute.For<IIdentityService>();
			_current = Substitute.For<ICurrentUserService>();
			_notification = Substitute.For<IChatNotificationService>();
			_sut = new SetOnlineStatusCommandHandler(_identity, _current, _notification); // system under test
		}

		[Test]
		public async Task No_user_returns_success_without_touching_identity()
		{
			_current.UserId.Returns((Guid?)null);

			var result = await _sut.Handle(new SetOnlineStatusCommand(true), CancellationToken.None);

			ResultAssert.IsSuccess(result);
			await _identity.DidNotReceiveWithAnyArgs().SetOnlineStatusAsync(default, default, default);
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task With_user_delegates_to_identity(bool isOnline)
		{
			var id = Guid.NewGuid();
			_current.UserId.Returns(id);
			_identity.SetOnlineStatusAsync(id, isOnline, Arg.Any<CancellationToken>()).Returns(Result.Success());

			var result = await _sut.Handle(new SetOnlineStatusCommand(isOnline), CancellationToken.None);

			ResultAssert.IsSuccess(result);
			await _identity.Received(1).SetOnlineStatusAsync(id, isOnline, Arg.Any<CancellationToken>());
		}
	}
}