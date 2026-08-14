using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Common
{
	public interface IDomainEvent : INotification
	{
		DateTime OccurredOn { get; }
	}
}
