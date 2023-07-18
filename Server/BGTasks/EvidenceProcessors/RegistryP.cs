using Server.BGTasks.EvidenceModels;
using Server.BGTasks;
using OpenQA.Selenium.Firefox;
using static Server.BGTasks.SharedBGMethods;
using System.Text;
using System.Text.Json;

namespace Server.BGTasks.EvidenceProcessors
{
    public class RegistryP : EvidenceChecker.IEvidenceWoker
    {
        public int score { get; set; } = 0;
        public int evidenceId { get; set; }
        public string reasonForScore { get; set; }
        public string additionalOutput { get; set; }
        public bool isProccessed { get; set; } = false;


        const bool debugMode = true;

        public async Task Process(Dictionary<string, string> data)
        {
            string decompressedData = await DecompressAsync(data["raw"]);
            List<RegistryModel> launchedPrograms = await JsonSerializer.DeserializeAsync<List<RegistryModel>>(new MemoryStream(Encoding.UTF8.GetBytes(decompressedData)));
            List<RegistryModel> filesToSeaerch = launchedPrograms.DistinctBy(l=>Path.GetFileName(l.FilePath)).Where(l=>l.RunCount<10).ToList();

            List<string> susFiles = new List<string>();
            List<string> whiteListFiles = new List<string>();

            FirefoxOptions options = new FirefoxOptions();
            options.AddArgument("--headless");
            try
            {
                using (FirefoxDriver driver = new FirefoxDriver(options))
                {
                    foreach (var file in filesToSeaerch)
                    {
                        string fileName = Path.GetFileName(file.FilePath);
                        if (data["aditionalData"].Contains(fileName, StringComparison.OrdinalIgnoreCase)) continue;

                        if (await SearchDuckDuckGo(driver, fileName))
                        {
                            susFiles.Add(fileName);
                            Log($"Found sus file - {fileName}");
                        }
                        else
                        {
                            whiteListFiles.Add(fileName);
                            Log($"Whitelisted this file - {fileName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Finished with error - {ex.Message}");
            }
            Log("Finished");

            if (susFiles.Count > 0)
            {
                score = 70;
                reasonForScore = $"I found suspicious files that were running on PC: \n{string.Join('\n', susFiles)}";
            }
            else
            {
                reasonForScore = "Didn't find anything suspicious";
            }

            additionalOutput = string.Join('\n', whiteListFiles);
            susFiles.Clear();
            whiteListFiles.Clear();
            launchedPrograms.Clear();
            isProccessed = true;
        }

        public static void Log(string message)
        {
            if (debugMode)
            {
                Console.WriteLine("[RegistryProcessor] " + message);
            }
        }
    }
}
