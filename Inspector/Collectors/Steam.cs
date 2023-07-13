using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Inspector.Collectors
{
    internal class Steam
    {

        private static string GetSteamLocation()
        {
            const string steamPathX64 = @"SOFTWARE\Wow6432Node\Valve\Steam";
            const string steamPathX32 = @"Software\Valve\Steam";
            string result = string.Empty;
            try
            {
                bool isX64OperationSystem = Environment.Is64BitOperatingSystem;
                RegistryKey getBaseRegDir = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, isX64OperationSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                result = getBaseRegDir.OpenSubKey(isX64OperationSystem ? steamPathX64 : steamPathX32, isX64OperationSystem)
                    ?.GetValue(isX64OperationSystem ? "InstallPath" : "SourceModInstallPath")?.ToString();
                getBaseRegDir.Close();
            }
            catch (Exception exception) { 
                Console.WriteLine(exception.Message); 
            }
            return result;
        }

        // метод, который получает id всех аккаунтов на котором происзодал авторизация
        public static List<string> GetSteamIdFromCoPlay()
        {
            List<string> files = Directory.GetFiles($"{GetSteamLocation()}\\config\\", "*.vdf")
                .Where(path => Regex.IsMatch(path, "\\d{17}")).Select(Path.GetFileName).ToList();
            var result = files.ConvertAll(item => Regex.Matches(item, @"_7656(.*?).vdf").Cast<Match>().Select(x => "7656" + x.Groups[1].Value).ToList()[0]);
            return result;
        }

        public static async Task Collect()
        {
            var IDs = GetSteamIdFromCoPlay();
            string res = SharedMethods.ToJson(IDs);
            await Requests.SendEvidence("SteamAccounts", res);
        }


        public static async Task<steamData> PeParseFromSteam(string steamId, bool isDeleted)
        {
            string url = $"https://steamcommunity.com/profiles/{steamId}";
            string username = "", avatarUrl = "";
            int level = 1;
            bool isHideProfile = false;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string text = await client.GetStringAsync(url).ConfigureAwait(false);
                    isHideProfile = Regex.Match(text, @"<div\s+class=""profile_private_info"">([^"">]+)</div>")?.Groups[1].Length > 0;
                    username = Regex.Match(text, @"<span class=""actual_persona_name"">([^"">]+)</span>")?.Groups[1].Value;
                    if (!isHideProfile)
                    {
                        int.TryParse(Regex.Match(text, @"<span class=""friendPlayerLevelNum"">([^"">]+)</span>")?.Groups[1].Value, out level);
                    }
                    var avatarUrlArray = Regex.Matches(text, @"<img src=""([^"">]+)"">").Cast<Match>().Select(x => x.Groups[1].Value).ToList();
                    foreach (string img in avatarUrlArray)
                    {
                        if (img.Contains("_full"))
                        {
                            avatarUrl = img;
                            break;
                        }
                    }

                }
            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine(exception.Message);
            }

            return new steamData(username, steamId, level, avatarUrl, isHideProfile, isDeleted);
        }

        public class steamData
        {
            public string username;
            public string steamId;
            int level;
            public string avatarUrl;
            public bool isHideProfile;
            public bool isDeleted;
            public steamData(string username, string steamId,int level, string avatarUrl, bool isHideProfile, bool isDeleted)
            {
                this.username = username;
                this.steamId = steamId;
                this.level = level;
                this.avatarUrl = avatarUrl;
                this.isHideProfile = isHideProfile;
                this.isDeleted = isDeleted;
            }
        }
    }
}
