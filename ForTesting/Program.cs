using DuckSharp;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;

namespace ForTesting
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			HttpClient client = new HttpClient();
			var s = await client.GetStringAsync("https://duckduckgo.com/?q=%22Op-DEVENV.EXE%22&t=h_&ia=web");
			File.WriteAllText("res.html", s);
			Console.WriteLine(s);
		}
	}
}