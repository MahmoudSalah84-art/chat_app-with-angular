using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Application.Common.DTO
{
	public sealed record AttachmentDto(string Url, string FileName, string ContentType, long SizeInBytes);

}
