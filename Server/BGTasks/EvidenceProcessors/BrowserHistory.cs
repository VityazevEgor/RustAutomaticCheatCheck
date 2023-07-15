using Server.BGTasks;
using System.Text;
using Server.BGTasks.EvidenceModels;
using System.Text.Json;

namespace Server.BGTasks.EvidenceProcessors
{
	public class BrowserHistory : EvidenceChecker.IEvidenceWoker
	{
		public int score { get; set; } = 0;
		public int evidenceId { get; set; }
		public string reasonForScore { get; set; }
		public string additionalOutput { get; set; }
		public bool isProccessed { get; set; } = false;

		private static string[] badTags = new string[] {
			"cheat",
			"hack",
			"macros",
			"чит",
			"макрос",
			"macro"
		};

		private static string[] whiteListUrls = new string[]
		{
			"youtube.com",
			"ya.ru",
			"google.com",
			"bing.com",
			"yandex.ru",
			"cyberforum.ru",
			"mintmanga.live",
			"vk.com",
			"github.com"
		};

		private static string[] whiteListTags = new string[] {
			"читать"
		};
		public async Task Process(Dictionary<string, string> data)
		{
			var result = await getCheatSites(data["raw"]);
			if (result.Count > 0)
			{
				score = 10;
				reasonForScore = "User visited sites releated to cheats: \n"+string.Join('\n', result.Select(r=> new Uri(r.Url).Host).Distinct());
			}
			else
			{
				reasonForScore = "The user did not visit sites dedicated to cheats";
			}
			isProccessed = true;
		}

		public static async Task<List<BrowserHistoryModel>> getCheatSites(string compressedData)
		{
			string decompressedData = await SharedBGMethods.DecompressAsync(compressedData);
			List<BrowserHistoryModel> history = await JsonSerializer.DeserializeAsync<List<BrowserHistoryModel>>(new MemoryStream(Encoding.UTF8.GetBytes(decompressedData)));
			var result = history.Where(h =>
											badTags.Any(t => h.Title.Contains(t, StringComparison.OrdinalIgnoreCase)) &&
											!whiteListUrls.Any(u => h.Url.Contains(u, StringComparison.OrdinalIgnoreCase)) &&
											!whiteListTags.Any(t => h.Title.Contains(t, StringComparison.OrdinalIgnoreCase))
										).ToList();
			return result;
		}
	}
}
