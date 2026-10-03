using chatme.Domain.Enums;

namespace chatme.Application.Common
{
	public static class AttachmentRules
	{
		public const long MaxSizeBytes = 25 * 1024 * 1024; // 25 MB

		private static readonly Dictionary<string, MessageType> Allowed = new(StringComparer.OrdinalIgnoreCase)
		{
			[".jpg"] = MessageType.Image,
			[".jpeg"] = MessageType.Image,
			[".png"] = MessageType.Image,
			[".gif"] = MessageType.Image,
			[".webp"] = MessageType.Image,

			[".mp4"] = MessageType.Video,
			[".mov"] = MessageType.Video,
			[".webm"] = MessageType.Video,

			[".mp3"] = MessageType.Audio,
			[".wav"] = MessageType.Audio,
			[".ogg"] = MessageType.Audio,
			[".m4a"] = MessageType.Audio,
			[".aac"] = MessageType.Audio,

			[".pdf"] = MessageType.Document,
			[".doc"] = MessageType.Document,
			[".docx"] = MessageType.Document,
			[".xls"] = MessageType.Document,
			[".xlsx"] = MessageType.Document,
			[".ppt"] = MessageType.Document,
			[".pptx"] = MessageType.Document,
			[".txt"] = MessageType.Document,
			[".csv"] = MessageType.Document,
			[".zip"] = MessageType.Document,
			[".rar"] = MessageType.Document,
			[".7z"] = MessageType.Document,
		};

		public static MessageType? Resolve(string fileName) =>
			Allowed.TryGetValue(Path.GetExtension(fileName), out var type) ? type : null;

		public static bool IsAllowed(string fileName) => Resolve(fileName) is not null;
	}
}