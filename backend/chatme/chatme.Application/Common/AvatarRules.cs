namespace chatme.Application.Common;

public static class AvatarRules
{
	public const long MaxSizeBytes = 5 * 1024 * 1024;

	private static readonly HashSet<string> AllowedExtensions =
		new(StringComparer.OrdinalIgnoreCase)
		{
			".jpg",
			".jpeg",
			".png",
			".webp"
		};

	public static bool IsAllowedExtension(string fileName)
	{
		return !string.IsNullOrWhiteSpace(fileName)
			&& AllowedExtensions.Contains(
				Path.GetExtension(fileName));
	}

	public static async Task<bool> HasImageSignatureAsync(
		Stream stream,
		CancellationToken cancellationToken = default)
	{
		if (!stream.CanSeek)
			return false;

		var originalPosition = stream.Position;

		try
		{
			stream.Position = 0;

			var header = new byte[12];
			var read = 0;

			while (read < header.Length)
			{
				var count = await stream.ReadAsync(
					header.AsMemory(read),
					cancellationToken);

				if (count == 0)
					break;

				read += count;
			}

			if (read < 12)
				return false;

			var isJpeg =
				header[0] == 0xFF &&
				header[1] == 0xD8 &&
				header[2] == 0xFF;

			var isPng =
				header[0] == 0x89 &&
				header[1] == 0x50 &&
				header[2] == 0x4E &&
				header[3] == 0x47 &&
				header[4] == 0x0D &&
				header[5] == 0x0A &&
				header[6] == 0x1A &&
				header[7] == 0x0A;

			var isWebp =
				header[0] == 'R' &&
				header[1] == 'I' &&
				header[2] == 'F' &&
				header[3] == 'F' &&
				header[8] == 'W' &&
				header[9] == 'E' &&
				header[10] == 'B' &&
				header[11] == 'P';

			return isJpeg || isPng || isWebp;
		}
		finally
		{
			stream.Position = originalPosition;
		}
	}
}