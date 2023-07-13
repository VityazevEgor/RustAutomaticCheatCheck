using Server.BGTasks.EvidenceModels;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace Server.BGTasks.EvidenceProcessors
{
	public class USBDevices
	{
		public static async Task<(int, string)> Process(string content, string runHistory)
		{
			var USBDevices = ParseXml(content).Where(d=>d.DeviceType.ToLower().Contains("storage"));
			List<LaunchEventInfoModel> events = await JsonSerializer.DeserializeAsync<List<LaunchEventInfoModel>>(new MemoryStream(Encoding.UTF8.GetBytes(runHistory)));
			var filteredEvents = events.Where(e => e.FileName.EndsWith("rust.exe", StringComparison.OrdinalIgnoreCase) || e.FileName.EndsWith("rustclient.exe", StringComparison.OrdinalIgnoreCase));
			string reason = string.Empty;
			
			foreach (var device in USBDevices)
			{
				//Console.WriteLine($"{device.DeviceType} | {device.Description} | {device.CreatedDate} | {device.LastPlugUnplugDate}");
				var foundItems = events.Where(e => (device.CreatedDate - e.RunTime <= TimeSpan.FromMinutes(10) && device.CreatedDate - e.RunTime>TimeSpan.FromSeconds(20)) 
												|| (device.LastPlugUnplugDate - e.RunTime <= TimeSpan.FromMinutes(10) && device.LastPlugUnplugDate - e.RunTime>TimeSpan.FromSeconds(20)));
				if (foundItems.Count() > 0)
				{
					reason += $"USB Device {device.Description} was plugged/unplaged before rust start {device.CreatedDate} or {device.LastPlugUnplugDate}.\n";
				}
			}
			if (!string.IsNullOrEmpty(reason))
			{
				return (20, reason);
			}
			else
			{
				return (0, "The user did not perform any actions with USB devices before launching Rust");
			}
		}

		private static List<USBDevicesModel> ParseXml(string xmlString)
		{
			var resultList = new List<USBDevicesModel>();

			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlString);

			foreach (XmlNode itemNode in xmlDoc.GetElementsByTagName("item"))
			{
				var device = new USBDevicesModel();

				device.Description = itemNode.SelectSingleNode("description")?.InnerText;
				device.DeviceType = itemNode.SelectSingleNode("device_type")?.InnerText;
				device.Connected = itemNode.SelectSingleNode("connected")?.InnerText;
				device.CreatedDate = ConvertToFileTime(itemNode.SelectSingleNode("registry_time_1")?.InnerText);
				device.LastPlugUnplugDate = ConvertToFileTime(itemNode.SelectSingleNode("registry_time_2")?.InnerText);

				resultList.Add(device);
			}

			return resultList;
		}

		private static DateTime? ConvertToFileTime(string fileTime)
		{
			if (string.IsNullOrEmpty(fileTime))
			{
				return null;
			}

			if (DateTime.TryParse(fileTime, out var date))
			{
				return date;
			}

			return null;
		}

	}
}
