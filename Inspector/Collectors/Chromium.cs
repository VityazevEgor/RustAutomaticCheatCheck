using Inspector.Models;
using System.Data.SQLite;

namespace Inspector.Collectors
{
	internal class Chromium
	{
		static Dictionary<string, string> rootPaths = new Dictionary<string, string>()
			{
				{"Chrome", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\"},
				{"Edge",  Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\Edge\User Data\"},
				{"Brave", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\BraveSoftware\Brave-Browser\User Data\"},
				{"Opera GX",  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Opera Software\Opera GX Stable\"},
				{"Opera",  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Opera Software\Opera Stable\"}
			};
		static List<string> getHistoryPath()
		{
			var result = new List<string>();
			foreach (var rootPath in rootPaths)
			{
				if (!Directory.Exists(rootPath.Value)) continue;

				string[] profileDirectories = Directory.GetDirectories(rootPath.Value, "Profile *");
				foreach (string profileDirectory in profileDirectories)
				{
					string historyPath = Path.Combine(profileDirectory, "History");
					if (File.Exists(historyPath))
					{
						result.Add(historyPath);
					}
				}
				string defaultHistory = Path.Combine(rootPath.Value, "Default", "History");
				if (File.Exists(defaultHistory))
				{
					result.Add(defaultHistory);
				}
			}
			return result;
		}

		public static async Task Collect()
		{
			var historyPaths = getHistoryPath();
			List<BrowserHistoryModel> browserHistoryModels = new List<BrowserHistoryModel>();
			List<DownloadHistoryModel> downloadHistoryModels = new List<DownloadHistoryModel>();
			Parallel.ForEach(historyPaths, historyPath =>
			{
				string copyPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				File.Copy(historyPath, copyPath, true);

				using (var connection = new SQLiteConnection($"Data Source={copyPath};Version=3;"))
				{
					connection.Open();

					// читаем историю посещения
					using (var command = new SQLiteCommand("SELECT id, url, title, visit_count, typed_count, last_visit_time, hidden FROM urls", connection))
					{
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								BrowserHistoryModel history = new BrowserHistoryModel();
								history.Title = reader.GetString(2);
								history.Url = reader.GetString(1);
								history.VisitCount = reader.GetInt32(3);

								long unixTimeMicroseconds = reader.GetInt64(5);
								long unixTimeSeconds = unixTimeMicroseconds / 1000000L;
								history.LastVisitTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;

								browserHistoryModels.Add(history);
							}
						}
					}

					// читаем иcторию загрузок
					using (var commands = new SQLiteCommand("SELECT target_path, start_time, tab_url, tab_referrer_url FROM downloads", connection))
					{
						using (var reader = commands.ExecuteReader())
						{
							while (reader.Read())
							{
								DownloadHistoryModel dwHistory = new DownloadHistoryModel();
								dwHistory.FileName = Path.GetFileName(reader.GetString(0));
								dwHistory.Url = reader.GetString(2);
								dwHistory.ReferrerUrl = reader.GetString(3);

								long unixTimeMicroseconds = reader.GetInt64(1);
								long unixTimeSeconds = unixTimeMicroseconds / 1000000L;
								dwHistory.StartTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;

								downloadHistoryModels.Add(dwHistory);
							}
						}
					}

				}
				File.Delete(copyPath);
			});
			string historyResult = SharedMethods.ToJson(browserHistoryModels.OrderBy(b=>b.LastVisitTime));
			string dwHisotoryResult = SharedMethods.ToJson(downloadHistoryModels.OrderBy(b=>b.StartTime));
			await Requests.SendEvidence("BrowserHistory", historyResult, true);
			await Requests.SendEvidence("DownloadHistory", dwHisotoryResult, true);
			//Console.WriteLine(dwHisotoryResult);
		}
	}
}
