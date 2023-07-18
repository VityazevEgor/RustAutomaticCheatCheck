using Prefetch;
using System.Collections.Concurrent;

namespace Inspector.Collectors
{
	internal class LaunchHistory
	{
		const string prefetchDir = @"C:\Windows\Prefetch\";

		public static async Task Collect()
		{
            await PrefetchCollect();
            await AppUsageCollect();
		}

		private static async Task PrefetchCollect()
		{
            string[] files = Directory.GetFiles(prefetchDir, "*.pf");
            ConcurrentBag<Models.LaunchEventInfoModel> events = new ConcurrentBag<Models.LaunchEventInfoModel>();
            Parallel.ForEach(files, file =>
            {
                var prefetch = PrefetchFile.Open(file);
                foreach (var runTime in prefetch.LastRunTimes)
                {
                    events.Add(new Models.LaunchEventInfoModel(prefetch.Header.ExecutableFilename, runTime.ToLocalTime().DateTime));
                }
            });
            var sortedEvents = events.OrderByDescending(e => e.RunTime).ToList();
            string res = SharedMethods.ToJson(sortedEvents);
            await Requests.SendEvidence("RunHistory", res);
        }

        private static async Task AppUsageCollect()
        {
            string xml = await SharedMethods.NirSoftEx("https://www.nirsoft.net/utils/appresourcesusageview.zip", "AppResourcesUsageView.exe", "/sort ~Timestamp");
            xml = await SharedMethods.CutNirSoftXML(xml, 10000);
            await Requests.SendEvidence("ResourceUsage", xml, true);
        }

	}
}
