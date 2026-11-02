using System.Security.Cryptography;
using System.Text;

namespace chatme.Application.Common.Helper
{
	public static class ReceiptTokenHelper
	{
		public static string Generate()
		{
			var bytes = RandomNumberGenerator.GetBytes(32);

			return Convert.ToBase64String(bytes)
				.TrimEnd('=')
				.Replace('+', '-')
				.Replace('/', '_');
		}

		public static string Hash(string token)
		{
			var hash = SHA256.HashData(
				Encoding.UTF8.GetBytes(token));

			return Convert.ToHexString(hash);
		}

		public static bool Verify(string token, string expectedHash)
		{
			var actualHash = Hash(token);

			try
			{
				return CryptographicOperations.FixedTimeEquals(
					Convert.FromHexString(actualHash),
					Convert.FromHexString(expectedHash));
			}
			catch (FormatException)
			{
				return false;
			}
		}
	}
}
