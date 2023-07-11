using System.Runtime.Serialization;

namespace Server.BGTasks.EvidenceModels
{
	[DataContract]
	public class LaunchEventInfoModel
	{
		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public DateTimeOffset RunTime { get; set; }

		public LaunchEventInfoModel(string FileName, DateTimeOffset RunTime)
		{
			this.FileName = FileName;
			this.RunTime = RunTime;
		}
	}
}
