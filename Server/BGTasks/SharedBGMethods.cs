using System.IO.Compression;
using System.Text;

namespace Server.BGTasks
{
	public class SharedBGMethods
	{
		public static async Task<string> DecompressAsync(string compressedString)
		{
			byte[] compressedBytes = Convert.FromBase64String(compressedString);
			using (MemoryStream ms = new MemoryStream(compressedBytes))
			{
				using (GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress))
				{
					byte[] decompressedBytes = new byte[4096];
					using (MemoryStream msOut = new MemoryStream())
					{
						int bytesRead;
						while ((bytesRead = await gzip.ReadAsync(decompressedBytes, 0, decompressedBytes.Length)) > 0)
						{
							await msOut.WriteAsync(decompressedBytes, 0, bytesRead);
						}
						byte[] decompressedBytesFinal = msOut.ToArray();
						return Encoding.UTF8.GetString(decompressedBytesFinal);
					}
				}
			}
		}
	}
}
