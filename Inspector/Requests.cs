using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public static async Task<bool> SendEvidence(string type, string content)
		{
			using (HttpClient client = new HttpClient())
			{
				var values = new Dictionary<string, string>
				{
					{ "steamId", currentSteamId },
					{ "type", type },
					{ "data", content }
				};

				var contentData = new FormUrlEncodedContent(values);

				var response = await client.PostAsync($"{baseUrl}APIEvidence/getEvidence", contentData);
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

	}
}
