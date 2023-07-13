using Prefetch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inspector.Collectors
{
	internal class LaunchHistory
	{
		const string prefetchDir = @"C:\Windows\Prefetch\";

		public static async Task Collect()
		{
			string[] files = Directory.GetFiles(prefetchDir, "*.pf");
			ConcurrentBag<Models.LaunchEventInfoModel> events = new ConcurrentBag<Models.LaunchEventInfoModel>();
			Parallel.ForEach(files, file =>
			{
				var prefetch = PrefetchFile.Open(file);
				foreach (var runTime in prefetch.LastRunTimes)
				{
					events.Add(new Models.LaunchEventInfoModel(prefetch.Header.ExecutableFilename, runTime.ToLocalTime().DateTime));
					//Console.WriteLine($"{prefetch.Header.ExecutableFilename} | {runTime.LocalDateTime}");
				}
			});
			var sortedEvents = events.OrderByDescending(e => e.RunTime).ToList();
			string res = SharedMethods.ToJson(sortedEvents);
			File.WriteAllText("a.txt", res);
			await Requests.SendEvidence("RunHistory", res);
		}
	}
}
