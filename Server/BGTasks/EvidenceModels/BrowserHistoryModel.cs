using System.Runtime.Serialization;

namespace Server.BGTasks.EvidenceModels
{
	[DataContract]
	public class BrowserHistoryModel
	{
		[DataMember]
		public string Title { get; set; }
		[DataMember]
		public string Url { get; set; }
		[DataMember]
		public int VisitCount { get; set; }
		[DataMember]
		public DateTime LastVisitTime { get; set; }
	}
}
