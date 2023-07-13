using System.Diagnostics;
using System.IO.Compression;

namespace Inspector.Collectors
{
	internal class USBDevices
	{
		private const string usbdeviewurl = "https://www.nirsoft.net/utils/usbdeview-x64.zip";
		private static string devievPath = Path.Combine(Path.GetTempPath(), "USBDeview.exe");
		private static string devievZipPath = Path.Combine(Path.GetTempPath(), "deview.zip");
		public static async Task Collect()
		{
			if (!File.Exists(devievPath))
			{
				using (HttpClient client = new HttpClient())
				{
					var response = await client.GetAsync(usbdeviewurl);
					if (response.IsSuccessStatusCode)
					{
						using var fileStream = await response.Content.ReadAsStreamAsync();
						using var file = new FileStream(devievZipPath, FileMode.CreateNew);
						await fileStream.CopyToAsync(file);
					}
					else
					{
						Console.WriteLine("Can't download usbdeview");
						return;
					}
				}
				ZipFile.ExtractToDirectory(devievZipPath, Path.GetTempPath());
				File.Delete(devievZipPath);
			}
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = devievPath,
					Arguments = "/sxml history.xml",
					UseShellExecute = false,
					WorkingDirectory = Path.GetTempPath()
				}
			};
			process.Start();
			await process.WaitForExitAsync();

			await Requests.SendEvidence("USBDevices", File.ReadAllText(Path.Combine(Path.GetTempPath(), "history.xml")));
		}
	}
}