using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using Server.BGTasks.EvidenceModels;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;
using Server.BGTasks;

namespace Server.BGTasks.EvidenceProcessors
{
	public class RunHisoty : EvidenceChecker.IEvidenceWoker
	{
		const bool debugMode = true;

		public int score { get; set; } = 0;
		public int evidenceId { get; set; }
		public string reasonForScore { get; set; }
		public string additionalOutput { get; set; }
		public bool isProccessed { get; set; } = false;

		public async Task Process(Dictionary<string, string> data)
		{
			FirefoxOptions options = new FirefoxOptions();
			options.AddArgument("--headless");
			List<string> susFiles = new List<string>();
			List<string> whiteListFiles = new List<string>();

			using (FirefoxDriver driver = new FirefoxDriver(options))
			{

				List<LaunchEventInfoModel> events = await JsonSerializer.DeserializeAsync<List<LaunchEventInfoModel>>(new MemoryStream(Encoding.UTF8.GetBytes(data["raw"])));
				var exeEvents = events.Where(e => e.FileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).OrderByDescending(e => e.RunTime).ToList();

				foreach (var exeEvent in exeEvents)
				{
					if (exeEvent.FileName.EndsWith("rust.exe", StringComparison.OrdinalIgnoreCase) || exeEvent.FileName.EndsWith("rustclient.exe", StringComparison.OrdinalIgnoreCase))
					{
						int startIndex = exeEvents.IndexOf(exeEvent);
						int endIndex = Math.Min(startIndex + 10, exeEvents.Count);
						for (int i = startIndex; i < endIndex; i++)
						{
							if (data["aditionalData"].Contains(exeEvents[i].FileName) || susFiles.Contains(exeEvents[i].FileName)) continue; // если уже ранее проверялось то скипаем
							if (await SearchDuckDuckGo(driver, exeEvents[i].FileName))
							{
								susFiles.Add(exeEvents[i].FileName);
								Log($"Found sus file - {exeEvents[i].FileName}");
							}
							else
							{
								whiteListFiles.Add(exeEvents[i].FileName);
							}
						}
					}
				}
				Log("Finished");
				if (susFiles.Count > 0)
				{
					score = 70;
					reasonForScore = $"I found suspicious files that were running before the Rust: \n{string.Join('\n', susFiles)}";
				}
				else
				{
					reasonForScore = "Didn't find anything suspicious";
				}
			}
			additionalOutput = string.Join('\n', whiteListFiles);
			susFiles.Clear();
			whiteListFiles.Clear();
			isProccessed = true;
		}
		private static async Task<bool> SearchDuckDuckGo(FirefoxDriver driver, string fileName)
		{
			string url = $"https://duckduckgo.com/?q=\"{Uri.EscapeDataString(fileName)}\"";
			driver.Navigate().GoToUrl(url);

			IJavaScriptExecutor js = driver;
			await WaitForPageLoad(js);
			
			return driver.PageSource.Contains("результаты не найдены");
		}

		private static async Task WaitForPageLoad(IJavaScriptExecutor js)
		{
			var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(4));
			var cancellationToken = cancellationTokenSource.Token;

			bool pageLoaded = false;
			while (!pageLoaded && !cancellationToken.IsCancellationRequested)
			{
				pageLoaded = (bool)js.ExecuteScript("return document.readyState == 'complete'");
				await Task.Delay(50, cancellationToken);
			}

			//if (!pageLoaded)
			//{
			//	throw new TimeoutException("Timeout while waiting for page to load.");
			//}
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
