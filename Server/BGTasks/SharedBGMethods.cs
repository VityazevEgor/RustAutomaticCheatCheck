using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using System.IO.Compression;
using System.Text;

namespace Server.BGTasks
{
	public class SharedBGMethods
	{
		public static async Task<string> DecompressAsync(string compressedString)
		{
			byte[] compressedBytes = Convert.FromBase64String(compressedString);
			using (MemoryStream ms = new MemoryStream(compressedBytes))
			{
				using (GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress))
				{
					byte[] decompressedBytes = new byte[4096];
					using (MemoryStream msOut = new MemoryStream())
					{
						int bytesRead;
						while ((bytesRead = await gzip.ReadAsync(decompressedBytes, 0, decompressedBytes.Length)) > 0)
						{
							await msOut.WriteAsync(decompressedBytes, 0, bytesRead);
						}
						byte[] decompressedBytesFinal = msOut.ToArray();
						return Encoding.UTF8.GetString(decompressedBytesFinal);
					}
				}
			}
		}

        public static async Task<bool> SearchDuckDuckGo(FirefoxDriver driver, string fileName)
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
    }
}
