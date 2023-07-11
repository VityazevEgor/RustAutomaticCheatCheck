using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using Server.BGTasks.EvidenceModels;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Server.BGTasks.EvidenceProcessors
{
	public class RunHisoty
	{
		const string promtTemplate = "You have a list of executable file names:\n{replaceMe}\nPrint the names of executable files that you do not know separated by commas. If there are no suitable names, then simply write “No” in the answer. You don't need to write explanations, just give an answer.";
		const string pattern = ".*\\.exe";
		public static async Task<(int,string)> Process(string conten)
		{
			FirefoxOptions options = new FirefoxOptions();
			options.AddArgument("--headless");
			using (IWebDriver driver = new FirefoxDriver(options))
			{
				List<LaunchEventInfoModel> events = JsonSerializer.Deserialize<List<LaunchEventInfoModel>>(conten).OrderByDescending(e => e.RunTime).ToList();
				for (int i = 0; i < events.Count; i++)
				{
					if (events[i].FileName.ToLower().Contains("rust.exe") || events[i].FileName.ToLower().Contains("rustclient.exe"))
					{
						for (int j = i; j < i + 10; j++)
						{
							if (events[j].FileName.ToLower().Contains(".exe"))
							{
								driver.Navigate().GoToUrl($"https://duckduckgo.com/?q=\"{events[j].FileName}\"");
								IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
								bool pageLoaded = (bool)js.ExecuteScript("return document.readyState == 'complete'");
								while (!pageLoaded)
								{
									pageLoaded = (bool)js.ExecuteScript("return document.readyState == 'complete'");
								}
								if (driver.PageSource.Contains("результаты не найдены"))
								{
									Console.WriteLine(events[j].FileName);
								}
							}
							else
							{
								Log("Не нашёл exe");
							}
						}
					}
				}
				Log("Finished");
				return (0, null);
			}
		}

		public static void Log(string message)
		{
			Console.WriteLine("[RunHistoryProcessor] "+message);
		}
	}
}
