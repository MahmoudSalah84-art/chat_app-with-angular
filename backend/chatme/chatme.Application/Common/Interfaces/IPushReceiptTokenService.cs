using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.Interfaces
{
	public interface IPushReceiptTokenService
	{
		Task<string> CreateAsync(
			Guid chatId,
			Guid messageId,
			Guid receiverId,
			CancellationToken cancellationToken);
	}
}
