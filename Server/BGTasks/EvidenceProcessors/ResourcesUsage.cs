using Microsoft.Extensions.Options;
using OpenQA.Selenium.Firefox;
using Server.BGTasks.EvidenceModels;
using System.Collections.Generic;
using System.Xml;
using static Server.BGTasks.SharedBGMethods;

namespace Server.BGTasks.EvidenceProcessors
{
    public class ResourcesUsage : EvidenceChecker.IEvidenceWoker
    {
        const bool debugMode = true;

        public int score { get; set; } = 0;
        public int evidenceId { get; set; }
        public string reasonForScore { get; set; }
        public string additionalOutput { get; set; }
        public bool isProccessed { get; set; } = false;

        public async Task Process(Dictionary<string, string> data)
        {
            string xml = await DecompressAsync(data["raw"]);
            string whiteListedNames = data["aditionalData"];
            List<LaunchEventInfoModel> logs = await ParseXml(xml);

            FirefoxOptions options = new FirefoxOptions();
            options.AddArgument("--headless");
            List<string> susFiles = new List<string>();
            List<string> whiteListFiles = new List<string>();

            using (FirefoxDriver driver = new FirefoxDriver(options))
            {
                foreach (var log in logs)
                {
                    if (log.FileName.EndsWith("rust.exe", StringComparison.OrdinalIgnoreCase) || log.FileName.EndsWith("rustclient.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        int startIndex = logs.IndexOf(log);
                        int endIndex = Math.Min(startIndex + 10, logs.Count);
                        Log($"Found rust start at {startIndex}");
                        for (int i=startIndex; i < endIndex; i++)
                        {
                            if (whiteListedNames.Contains(logs[i].FileName, StringComparison.OrdinalIgnoreCase)) continue;

                            if (await SearchDuckDuckGo(driver, logs[i].FileName))
                            {
                                susFiles.Add(logs[i].FileName);
                                Log($"Found sus file - {susFiles.Last()}");
                            }
                            else
                            {
                                whiteListFiles.Add(logs[i].FileName);
                                Log($"Whitelisted this file - {whiteListFiles.Last()}");
                            }
                        }
                    }
                }
            }

            if (susFiles.Count > 0)
            {
                score = 70;
                reasonForScore = $"I found suspicious files that were running before the Rust: \n{string.Join('\n', susFiles)}";
            }
            else
            {
                reasonForScore = "Didn't find anything suspicious";
            }
            additionalOutput = string.Join('\n', whiteListFiles);
            susFiles.Clear();
            whiteListFiles.Clear();
            isProccessed = true;
        }

        private async Task<List<LaunchEventInfoModel>> ParseXml(string xml)
        {
            List<LaunchEventInfoModel> list = new List<LaunchEventInfoModel>();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            foreach (XmlNode itemNode in xmlDoc.GetElementsByTagName("item"))
            {
                var logItem = new LaunchEventInfoModel();

                logItem.FileName = itemNode.SelectSingleNode("app_name")?.InnerText ?? string.Empty;
                logItem.RunTime = DateTime.Parse(itemNode.SelectSingleNode("timestamp")?.InnerText ?? "01.01.1990");

                if (logItem.FileName.Contains(" ")) logItem.FileName = logItem.FileName.Substring(0, logItem.FileName.IndexOf(" "));
                if (!logItem.FileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;

                logItem.FileName = Path.GetFileName(logItem.FileName);
                list.Add(logItem);
                //Console.WriteLine($"{logItem.FileName} - {logItem.RunTime}");
            }

            return list;

        }

        private async Task Log(string message)
        {
            if (debugMode)
            {
                Console.WriteLine("[ResourcesUsageProcessor] " + message);
            }
        }

    }
}
