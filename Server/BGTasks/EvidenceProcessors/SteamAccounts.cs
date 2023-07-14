using System.Text;
using System.Text.Json;
using Server.BGTasks;

namespace Server.BGTasks.EvidenceProcessors
{
	public class SteamAccounts : EvidenceChecker.IEvidenceWoker
	{
		const bool debugMode = true;

		public int score { get; set; } = 0;
		public int evidenceId { get; set; }
		public string reasonForScore { get; set; }
		public string additionalOutput { get; set; }
		public bool isProccessed { get; set; } = false;
		public async Task Process(Dictionary<string, string> data)
		{
			List<string> steamIDs = await JsonSerializer.DeserializeAsync<List<string>>(new MemoryStream(Encoding.UTF8.GetBytes(data["raw"])));
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
				score = 20;
				reasonForScore = $"Found account(s) with game bans: {string.Join(' ', bannedIDs)}";
			}
			else
			{
				reasonForScore = "No accounts with game bans";
			}
			isProccessed = true;
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
