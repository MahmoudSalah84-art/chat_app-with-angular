using chatme.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Features.Push.Commands.ConfirmPushReceipt
{
	public sealed record ConfirmPushReceiptCommand(
	string ReceiptToken
	) : IRequest<Result>;
}
