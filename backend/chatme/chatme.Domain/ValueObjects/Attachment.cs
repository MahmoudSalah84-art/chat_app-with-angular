namespace chatme.Domain.ValueObjects
{
	public sealed record Attachment(
		string Url,
		string PublicId,
		string FileName,
		string ContentType,
		long SizeInBytes);
}
