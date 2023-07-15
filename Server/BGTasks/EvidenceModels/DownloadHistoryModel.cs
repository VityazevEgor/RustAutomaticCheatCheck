using System.Runtime.Serialization;

namespace Server.BGTasks.EvidenceModels
{
	[DataContract]
	public class DownloadHistoryModel
	{
		[DataMember]
		public string FileName { get; set; }
		[DataMember]
		public string Url { get; set; }
		[DataMember]
		public string ReferrerUrl { get; set; }
		[DataMember]
		public DateTime StartTime { get; set; }
	}
}
