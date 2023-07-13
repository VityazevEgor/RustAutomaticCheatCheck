using System.Text;
using System.Text.Json;

namespace Server.BGTasks.EvidenceProcessors
{
	public class SteamAccounts
	{
		const bool debugMode = true;
		public static async Task<(int, string)> Process(string content)
		{
			List<string> steamIDs = await JsonSerializer.DeserializeAsync<List<string>>(new MemoryStream(Encoding.UTF8.GetBytes(content)));
			List<string> bannedIDs = new List<string>();
			using (HttpClient client = new HttpClient())
			{
				foreach (var steamID in steamIDs)
				{
					string url = $"https://steamcommunity.com/profiles/{steamID}";
					var html = await client.GetStringAsync(url);
					if (html.Contains("<div class=\"profile_ban\">"))
					{
						bannedIDs.Add(url);
					}
				}
			}
			
			if (bannedIDs.Count > 0)
			{
				return (20, $"Found account(s) with game bans: {string.Join(' ', bannedIDs)}");
			}
			else
			{
				return (0, "No accounts with game bans");
			}

		}

		public static async Task Log(string message)
		{
			if (debugMode)
			{
				Console.WriteLine("[RunHistoryProcessor] " + message);
			}
		}
	}
}
