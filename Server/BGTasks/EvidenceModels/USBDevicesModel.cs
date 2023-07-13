namespace Server.BGTasks.EvidenceModels
{
	public class USBDevicesModel
	{
		public string Description { get; set; }
		public string DeviceType { get; set; }
		public string Connected { get; set; }
		public DateTime? CreatedDate { get; set; }
		public DateTime? LastPlugUnplugDate { get; set; }
	}
}
