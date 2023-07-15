using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Models
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
