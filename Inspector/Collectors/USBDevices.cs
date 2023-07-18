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
			string result = await SharedMethods.NirSoftEx(usbdeviewurl, "USBDeview.exe");
			await Requests.SendEvidence("USBDevices", result);
		}
	}
}