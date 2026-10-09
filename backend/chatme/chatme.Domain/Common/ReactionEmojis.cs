namespace chatme.Domain.Common
{
	public static class ReactionEmojis
	{
		public static readonly IReadOnlyList<string> Allowed =
		[
			"❤️",			 // ❤️
			"\U0001F44D",     // 👍
			"\U0001F602",     // 😂
			"\U0001F62E",     // 😮
			"\U0001F622",     // 😢
			"\U0001F64F",     // 🙏
		];

		private static string Strip(string value) => value.Replace("️", string.Empty).Trim(); // Remove variation selectors and whitespace

		public static string? Normalize(string? emoji)
		{
			if (string.IsNullOrWhiteSpace(emoji)) return null;
			var key = Strip(emoji);
			return Allowed.FirstOrDefault(a => Strip(a) == key);
		}


		public static bool IsAllowed(string? emoji) => Normalize(emoji) is not null;
	}
}