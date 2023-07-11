using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Models
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
