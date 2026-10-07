
namespace chatme.Application.Common.DTO
{
	public sealed record ParticipantReceiptDto(Guid UserId, DateTime? LastDeliveredAt, DateTime? LastReadAt);
}
