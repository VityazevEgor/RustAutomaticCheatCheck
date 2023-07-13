using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using Server.BGTasks.EvidenceModels;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace Server.BGTasks.EvidenceProcessors
{
	public class RunHisoty
	{
		const string promtTemplate = "You have a list of executable file names:\n{replaceMe}\nPrint the names of executable files that you do not know separated by commas. If there are no suitable names, then simply write “No” in the answer. You don't need to write explanations, just give an answer.";
		const string pattern = ".*\\.exe";
		const bool debugMode = true;
		public static async Task<(int,string)> Process(string content)
		{
			FirefoxOptions options = new FirefoxOptions();
			options.AddArgument("--headless");
			List<string> susFiles = new List<string>();

			using (FirefoxDriver driver = new FirefoxDriver(options))
			{

				List<LaunchEventInfoModel> events = await JsonSerializer.DeserializeAsync<List<LaunchEventInfoModel>>(new MemoryStream(Encoding.UTF8.GetBytes(content)));
				var exeEvents = events.Where(e => e.FileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).OrderByDescending(e => e.RunTime).ToList();

				foreach (var exeEvent in exeEvents)
				{
					if (exeEvent.FileName.EndsWith("rust.exe", StringComparison.OrdinalIgnoreCase) || exeEvent.FileName.EndsWith("rustclient.exe", StringComparison.OrdinalIgnoreCase))
					{
						int startIndex = exeEvents.IndexOf(exeEvent);
						int endIndex = Math.Min(startIndex + 10, exeEvents.Count);
						for (int i=startIndex; i<endIndex ; i++)
						{
							if (await SearchDuckDuckGo(driver, exeEvents[i].FileName))
							{
								susFiles.Add(exeEvents[i].FileName);
								Log($"Found sus file - {exeEvents[i].FileName}");
							}
						}
					}
				}
				Log("Finished");
				if (susFiles.Count > 0)
				{
					return (70, $"I found suspicious files that were running before the Rust: {string.Join(',', susFiles)}");
				}
				else
				{
					return (0, "Didn't find anything suspicious");
				}
			}
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
