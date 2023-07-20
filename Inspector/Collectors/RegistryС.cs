using Microsoft.Win32;
using Inspector.Models;
using System.Xml;

namespace Inspector.Collectors
{
    internal class RegistryС
    {
        private static async Task<List<RegistryModel>> ReadAppSwitched()
        {
            return await Task.Run(() =>
            {
                List<RegistryModel> appList = new List<RegistryModel>();
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FeatureUsage\AppSwitched");
                if (key != null)
                {
                    foreach (string appName in key.GetValueNames())
                    {
                        if (!appName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;
                        int launchCount = Convert.ToInt32(key.GetValue(appName));
                        appList.Add(new RegistryModel {
                            FilePath = appName, 
                            RunCount = launchCount, 
                            stillExsist = File.Exists(appName),
                            fileSize = File.Exists(appName) ? new FileInfo(appName).Length /(1024.0 * 1024.0) : 0
                        });
                    }
                    key.Close();
                }
                return appList;
            });
        }

        private static async Task<List<RegistryModel>> ReadAppLaunch()
        {
            return await Task.Run(() =>
            {
                List<RegistryModel> appList = new List<RegistryModel>();
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FeatureUsage\AppLaunch");
                if (key != null)
                {
                    foreach (string appName in key.GetValueNames())
                    {
                        if (!appName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;
                        int launchCount = Convert.ToInt32(key.GetValue(appName));
                        appList.Add(new RegistryModel
                        {
                            FilePath = appName,
                            RunCount = launchCount,
                            stillExsist = File.Exists(appName),
                            fileSize = File.Exists(appName) ? new FileInfo(appName).Length / (1024.0 * 1024.0) : 0
                        });
                    }
                    key.Close();
                }
                return appList;
            });
        }

        private static async Task<List<RegistryModel>> ReadMuiChahce()
        {
            return await Task.Run(() =>
            {
                List<RegistryModel> appList = new List<RegistryModel>();
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache");
                if ( key != null)
                {
                    foreach (string appName in key.GetValueNames())
                    {
                        if (!appName.Contains(".")) continue;
                        string filteredAppName = appName.Substring(0, appName.LastIndexOf("."));
                        if (!filteredAppName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;
                        appList.Add(new RegistryModel
                        {
                            FilePath = filteredAppName,
                            RunCount = -1,
                            stillExsist = File.Exists(filteredAppName),
                            fileSize = File.Exists(filteredAppName) ? new FileInfo(filteredAppName).Length / (1024.0 * 1024.0) : 0
                        });
                    }
                }

                return appList;
            });
        }

        private static async Task<List<RegistryModel>> ReadStore()
        {
            return await Task.Run(() =>
            {
                List<RegistryModel> appList = new List<RegistryModel>();
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store");
                if (key != null)
                {
                    foreach (string appName in key.GetValueNames())
                    {
                        if (!appName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;
                        appList.Add(new RegistryModel
                        {
                            FilePath = appName,
                            RunCount = -1,
                            stillExsist = File.Exists(appName),
                            fileSize = File.Exists(appName) ? new FileInfo(appName).Length / (1024.0 * 1024.0) : 0
                        });
                    }
                }

                return appList;
            });
        }

        private static async Task<List<RegistryModel>> ReadUserAssist()
        {
            List<RegistryModel> appList = new List<RegistryModel>();

            string xml = await SharedMethods.NirSoftEx("https://www.nirsoft.net/utils/userassistview.zip", "UserAssistView.exe");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeList items = doc.GetElementsByTagName("item");
            foreach (XmlNode item in items)
            {
                string appName = item.SelectSingleNode("item_name")?.InnerText;
                if (!appName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ) continue;

                appList.Add(new RegistryModel { 
                    FilePath = appName, 
                    RunCount = -1, 
                    stillExsist=File.Exists(appName),
                    fileSize = File.Exists(appName) ? new FileInfo(appName).Length / (1024.0 * 1024.0) : 0
                });
            }
            return appList;
        }



        public static async Task Collect()
        {
            List<RegistryModel> result = await ReadAppSwitched();
            result.AddRange(await ReadAppLaunch());
            result.AddRange(await ReadMuiChahce());
            result.AddRange(await ReadStore());
            result.AddRange(await ReadUserAssist());
            var fileterdResult =  result.DistinctBy(r => r.FilePath);
            await Requests.SendEvidence("Registry", SharedMethods.ToJson(fileterdResult), true);
        }
    }
}
