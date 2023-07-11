using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Server.GptProviders
{
	public class acytoo
	{
		const string url = "https://chat.acytoo.com/api/completions";
		public static async Task<string> sendMessage(string message)
		{
			HttpClient client = new HttpClient();
			var data = new
			{
				key = "",
				model = "gpt-3.5-turbo",
				messages = new[]
			   {
					new
					{
						role = "user",
						content = message,
						createdAt = 1688518523500
					}
				},
				temperature = 1,
				password = ""
			};
			var json = JsonConvert.SerializeObject(data);

			var content = new StringContent(json, Encoding.UTF8, "application/json");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 Edg/114.0.1823.67");

			var response = await client.PostAsync(url, content);
			client.Dispose();
			if (response.IsSuccessStatusCode)
			{
				var responseContent = await response.Content.ReadAsStringAsync();
				return responseContent;
			}
			else
			{
				Console.WriteLine($"Failed to get completions. Status code: {response.StatusCode}");
				return null;
			}
		}
	}
}
