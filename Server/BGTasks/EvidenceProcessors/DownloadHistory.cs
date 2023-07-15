using System.Text;
using System.Text.Json;
using Server.BGTasks.EvidenceModels;
namespace Server.BGTasks.EvidenceProcessors
{
	public class DownloadHistory: EvidenceChecker.IEvidenceWoker
	{
		public int score { get; set; } = 0;
		public int evidenceId { get; set; }
		public string reasonForScore { get; set; }
		public string additionalOutput { get; set; }
		public bool isProccessed { get; set; } = false;

		public async Task Process(Dictionary<string, string> data)
		{
			var cheatSites = await BrowserHistory.getCheatSites(data["aditionalData"]);
			string decompressedData = await SharedBGMethods.DecompressAsync(data["raw"]);
			List<DownloadHistoryModel> history = await JsonSerializer.DeserializeAsync<List<DownloadHistoryModel>>(new MemoryStream(Encoding.UTF8.GetBytes(decompressedData)));
			var result = history.Where(h => cheatSites.Any(s => getHost(h.Url) == getHost(s.Url) || getHost(h.ReferrerUrl) == getHost(s.Url))).ToList();
			if (result.Count > 0)
			{
				score = 40;
				reasonForScore = "The user downloaded file from site releated to cheats: \n";
				foreach (var item in result)
				{
					reasonForScore += $"{item.FileName} from {getHost(item.ReferrerUrl)} or {getHost(item.Url)}\n";
				}
			}
			else
			{
				reasonForScore = "The user did not download files from sites related to cheats";
			}
			isProccessed = true;
		}

		private string getHost(string rawUrl)
		{
			if (string.IsNullOrEmpty(rawUrl)) return string.Empty;
			return new Uri(rawUrl).Host;
		}
	}
}
