using System.Runtime.Serialization;

namespace Server.BGTasks.EvidenceModels
{
	[DataContract]
	public class LaunchEventInfoModel
	{
		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public DateTime RunTime { get; set; }
	}
}
