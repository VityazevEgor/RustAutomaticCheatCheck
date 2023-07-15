using Server.BGTasks.EvidenceModels;
using System.Text;
using System.Text.Json;
using System.Xml;
using Server.BGTasks;

namespace Server.BGTasks.EvidenceProcessors
{
	public class USBDevices : EvidenceChecker.IEvidenceWoker
	{

		public int score { get; set; } = 0;
		public int evidenceId { get; set; }
		public string reasonForScore { get; set; }
		public string additionalOutput { get; set; }
		public bool isProccessed { get; set; } = false;
		public async Task Process(Dictionary<string, string> data)
		{
			var usbDevices = ParseXml(data["raw"]).Where(d => d.DeviceType.ToLower().Contains("storage"));
			var events = await JsonSerializer.DeserializeAsync<List<LaunchEventInfoModel>>(new MemoryStream(Encoding.UTF8.GetBytes(data["aditionalData"])));
			var filteredEvents = events.Where(e => e.FileName.EndsWith("rust.exe", StringComparison.OrdinalIgnoreCase) || e.FileName.EndsWith("rustclient.exe", StringComparison.OrdinalIgnoreCase));

			var foundDevices = usbDevices.Where(device =>
			{
				var foundItems = events.Where(e => (device.CreatedDate - e.RunTime <= TimeSpan.FromMinutes(10) && device.CreatedDate - e.RunTime > TimeSpan.FromSeconds(20))
										|| (device.LastPlugUnplugDate - e.RunTime <= TimeSpan.FromMinutes(10) && device.LastPlugUnplugDate - e.RunTime > TimeSpan.FromSeconds(20)));

				return foundItems.Count() > 0;
			});

			if (foundDevices.Any())
			{
				score = 20;
				reasonForScore = $"USB Device(s) that(those) was(were) plugged/unplugged before rust start: \n{string.Join("\n", foundDevices.Select(d => d.Description))}";
			}
			else
			{
				reasonForScore = "The user did not perform any actions with USB devices before launching Rust";
			}

			isProccessed = true;
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
