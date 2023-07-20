using System.IO.Compression;
using System.Text;

namespace Inspector
{
    internal class Requests
	{
		public static string currentSteamId = string.Empty;
		private const string baseUrl = "http://localhost:5079/";
		public static async Task<bool> ValidateSuspect(List<string> steamIDs)
		{
			using (HttpClient client = new HttpClient())
			{
				foreach (string steamID in steamIDs)
				{
					var response = await client.GetStringAsync($"{baseUrl}APIsusModels/Check/{steamID}");
					if (response.ToLower().Contains("true"))
					{
						currentSteamId = steamID;
						return true;
					}
				}
			}
			return false;
		}

		public static async Task<bool> SendEvidence(string type, string content, bool compress = false)
		{
			using (HttpClient client = new HttpClient())
			{
				var values = new Dictionary<string, string>
				{
					{ "steamId", currentSteamId },
					{ "type", type },
					{ "data", (compress? await CompressAsync(content) : content) }
				};

				var contentData = new FormUrlEncodedContent(values);
				Console.WriteLine(values["steamId"]);
				var response = await client.PostAsync($"{baseUrl}APIEvidence/getEvidence", contentData);
				Console.WriteLine(await response.Content.ReadAsStringAsync());
				if (response.IsSuccessStatusCode)
				{
					return true;
				}
				else
				{
					Console.WriteLine($"Failed to send evidence for Steam ID {currentSteamId}. StatusCode: {response.StatusCode}");
					return false;
				}
			}
		}


		private static async Task<string> CompressAsync(string originalString)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(originalString);
			using (MemoryStream ms = new MemoryStream())
			{
				using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
				{
					await gzip.WriteAsync(bytes, 0, bytes.Length);
				}
				byte[] compressedBytes = ms.ToArray();
				return Convert.ToBase64String(compressedBytes);
			}
		}

	}
}
