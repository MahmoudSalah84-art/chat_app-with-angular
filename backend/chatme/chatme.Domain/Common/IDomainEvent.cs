using MediatR;

namespace chatme.Domain.Common
{
	public interface IDomainEvent : INotification
	{
		DateTime OccurredOn { get; }
	}
}